using CleanBrilliant.Domain.Control;
using CleanBrilliant.Domain.DomainInterface;
using Microsoft.AspNetCore.Mvc;

namespace CleanBrilliant.Controllers
{
    [ApiController]
    [Route("api/carbon")]
    public class CarbonCalculationController : ControllerBase
    {
        private readonly TransportCarbonManager _transportCarbonManager;
        private readonly CarbonDataAggregator _carbonDataAggregator;
        private readonly CleanBrilliant.Domain.BoundaryInterface.ISupplierLookupGateway _supplierLookupGateway;
        private readonly OutboundDistribution _outboundDistribution;

        public CarbonCalculationController(
            TransportCarbonManager transportCarbonManager,
            CarbonDataAggregator carbonDataAggregator,
            CleanBrilliant.Domain.BoundaryInterface.ISupplierLookupGateway supplierLookupGateway,
            OutboundDistribution outboundDistribution)
        {
            _transportCarbonManager = transportCarbonManager;
            _carbonDataAggregator = carbonDataAggregator;
            _supplierLookupGateway = supplierLookupGateway;
            _outboundDistribution = outboundDistribution;
        }

        [HttpGet("order/{orderID}/shipping")]
        public async Task<IActionResult> GetOrderShippingCarbon([FromRoute] string orderID)
        {
            var shipping = await _transportCarbonManager.GetOrderShippingBreakdown(orderID);
            if (shipping == null)
            {
                return NotFound(new { message = $"No shipping carbon record found for order ID '{orderID}'." });
            }

            return Ok(shipping);
        }

        [HttpGet("restock/{restockID}/shipping")]
        public async Task<IActionResult> GetRestockShippingCarbon([FromRoute] string restockID)
        {
            var shipping = await _transportCarbonManager.GetRestockShippingBreakdown(restockID);
            if (shipping == null)
            {
                return NotFound(new { message = $"No shipping carbon record found for restock ID '{restockID}'." });
            }

            return Ok(shipping);
        }

        [HttpGet("order/{orderID}/total")]
        public async Task<IActionResult> GetOrderCarbon(
            [FromRoute] string orderID,
            [FromQuery] string countryCode = "",
            [FromQuery] string postalCode = "")
        {
            var factory = new CustomerEntityFactory();
            var totalCarbon = await _carbonDataAggregator.GetOrderCarbon(orderID, countryCode, postalCode, factory);
            return Ok(new { orderID, totalCarbon });
        }

        [HttpGet("restock/{restockID}/total")]
        public async Task<IActionResult> GetSupplierOrderCarbon(
            [FromRoute] string restockID,
            [FromQuery] string countryCode = "",
            [FromQuery] string postalCode = "")
        {
            var factory = new SupplierEntityFactory();
            var totalCarbon = await _carbonDataAggregator.GetSupplierOrderCarbon(restockID, postalCode, countryCode, factory);
            return Ok(new { restockID, totalCarbon });
        }

        [HttpGet("order/{orderID}/summary")]
        public async Task<IActionResult> GetOrderCarbonSummary([FromRoute] string orderID)
        {
            var summary = await _carbonDataAggregator.GetOrderCarbonSummaryBreakdown(orderID);
            if (summary == null)
            {
                return NotFound(new { message = $"No total carbon summary found for order ID '{orderID}'." });
            }

            return Ok(summary);
        }

        [HttpGet("restock/{restockID}/summary")]
        public async Task<IActionResult> GetRestockCarbonSummary([FromRoute] string restockID)
        {
            var summary = await _carbonDataAggregator.GetRestockCarbonSummaryBreakdown(restockID);
            if (summary == null)
            {
                return NotFound(new { message = $"No total carbon summary found for restock ID '{restockID}'." });
            }

            return Ok(summary);
        }

        [HttpGet("order/{orderID}/outbound-summary")]
        public async Task<IActionResult> GetOutboundSummary([FromRoute] string orderID)
        {
            var summary = await _transportCarbonManager.GetOutboundSummary(orderID);
            if (summary == null)
            {
                return NotFound(new { message = $"No outbound summary found for order ID '{orderID}'." });
            }

            return Ok(summary);
        }

        [HttpGet("restock/{restockID}/inbound-summary")]
        public async Task<IActionResult> GetInboundSummary([FromRoute] string restockID)
        {
            var summary = await _transportCarbonManager.GetInboundSummary(restockID);
            if (summary == null)
            {
                return NotFound(new { message = $"No inbound summary found for restock ID '{restockID}'." });
            }

            return Ok(summary);
        }

        [HttpGet("hubs")]
        public async Task<IActionResult> GetHubs()
        {
            var table = await _supplierLookupGateway.FindAll();
            var results = new List<object>();

            foreach (System.Data.DataRow row in table.Rows)
            {
                string supplierId = row.Table.Columns.Contains("supplier_id") && row["supplier_id"] != DBNull.Value
                    ? Convert.ToString(row["supplier_id"]) ?? string.Empty
                    : string.Empty;

                string name = GetString(row, "company_name")
                    ?? GetString(row, "supplier_name")
                    ?? GetString(row, "name")
                    ?? $"Supplier {supplierId}";

                string postalCode = GetString(row, "postal_code")
                    ?? GetString(row, "postalCode")
                    ?? "N/A";

                results.Add(new
                {
                    supplierId,
                    companyName = name,
                    postalCode
                });
            }

            return Ok(results);
        }

        [HttpGet("customer-distance")]
        public async Task<IActionResult> CalculateCustomerDistance(
            [FromQuery] string customerPostalCode,
            [FromQuery] string hubPostalCode,
            [FromQuery] string companyName = "")
        {
            if (string.IsNullOrWhiteSpace(customerPostalCode) || string.IsNullOrWhiteSpace(hubPostalCode))
            {
                return BadRequest(new { message = "Both customer postal code and hub postal code are required." });
            }

            try
            {
                var route = await _outboundDistribution.CalculatePostalRoute(customerPostalCode, hubPostalCode);

                return Ok(new
                {
                    customerPostalCode,
                    hubPostalCode,
                    companyName,
                    customerLatitude = Math.Round(route.SourceLatitude, 6),
                    customerLongitude = Math.Round(route.SourceLongitude, 6),
                    hubLatitude = Math.Round(route.DestinationLatitude, 6),
                    hubLongitude = Math.Round(route.DestinationLongitude, 6),
                    distanceKm = Math.Round(route.DistanceKm, 2),
                    durationMin = Math.Round(route.DurationMin, 2),
                    formula = route.Formula
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetCarbonLogs()
        {
            var logs = await _carbonDataAggregator.GetCarbonLogs();
            return Ok(logs);
        }

        private static string? GetString(System.Data.DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToString(row[columnName]);
        }
    }
}
