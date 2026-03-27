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
        private readonly InboundLogistics _inboundLogistics;

        public CarbonCalculationController(
            TransportCarbonManager transportCarbonManager,
            CarbonDataAggregator carbonDataAggregator,
            CleanBrilliant.Domain.BoundaryInterface.ISupplierLookupGateway supplierLookupGateway,
            OutboundDistribution outboundDistribution,
            InboundLogistics inboundLogistics)
        {
            _transportCarbonManager = transportCarbonManager;
            _carbonDataAggregator = carbonDataAggregator;
            _supplierLookupGateway = supplierLookupGateway;
            _outboundDistribution = outboundDistribution;
            _inboundLogistics = inboundLogistics;
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

        [HttpGet("inbound-warehouses")]
        public IActionResult GetInboundWarehouses()
        {
            var results = _inboundLogistics.GetSupportedWarehouses().Select(warehouse => new
            {
                warehouse.Id,
                warehouse.Name,
                warehouse.PostalCode,
                companyLocation = InboundLogistics.CompanyLocationName,
                companyPostalCode = InboundLogistics.CompanyPostalCode,
                shippingMethod = "truck"
            });

            return Ok(results);
        }

        [HttpGet("inbound-route")]
        public async Task<IActionResult> CalculateInboundRoute([FromQuery] string warehouseId)
        {
            if (string.IsNullOrWhiteSpace(warehouseId))
            {
                return BadRequest(new { message = "A supported warehouse ID is required." });
            }

            try
            {
                var warehouse = _inboundLogistics.GetSupportedWarehouse(warehouseId);
                if (warehouse == null)
                {
                    return BadRequest(new
                    {
                        message = "Inbound logistics only supports SIT Campus W Block, SIT Dover, and SIT NYP."
                    });
                }

                var route = await _inboundLogistics.CalculateInboundRoute(warehouseId);
                string? restockId = Request.Query["restockID"].FirstOrDefault();

                float? loggedCarbon = null;
                if (!string.IsNullOrWhiteSpace(restockId))
                {
                    await _inboundLogistics.LogCalculatedRoute(restockId, route);
                    loggedCarbon = await _transportCarbonManager.CalculateSupplierDistanceCarbon(route.DistanceKm, "truck");
                    await _transportCarbonManager.LogRestockEmission(restockId, loggedCarbon.Value);
                }

                return Ok(new
                {
                    companyName = InboundLogistics.CompanyLocationName,
                    companyPostalCode = InboundLogistics.CompanyPostalCode,
                    restockID = restockId,
                    warehouseId = warehouse.Id,
                    warehouseName = warehouse.Name,
                    warehousePostalCode = warehouse.PostalCode,
                    shippingMethod = "truck",
                    routeChain = $"{warehouse.Name} -> {InboundLogistics.CompanyLocationName}",
                    sourceLatitude = Math.Round(route.SourceLatitude, 6),
                    sourceLongitude = Math.Round(route.SourceLongitude, 6),
                    destinationLatitude = Math.Round(route.DestinationLatitude, 6),
                    destinationLongitude = Math.Round(route.DestinationLongitude, 6),
                    distanceKm = Math.Round(route.DistanceKm, 2),
                    durationMin = Math.Round(route.DurationMin, 2),
                    formula = route.Formula,
                    loggedTransportCarbon = loggedCarbon.HasValue ? Math.Round((double)loggedCarbon.Value, 4) : (double?)null
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

        [HttpGet("outbound-route")]
        public async Task<IActionResult> CalculateOutboundRoute(
            [FromQuery] string customerPostalCode,
            [FromQuery] string method = "truck",
            [FromQuery] string countryCode = "SG",
            [FromQuery] string? orderID = null)
        {
            var normalizedMethod = NormalizeShippingMethod(method);
            var malaysiaDestination = IsMalaysiaDestination(countryCode);

            if (string.IsNullOrWhiteSpace(customerPostalCode) &&
                (!malaysiaDestination || string.Equals(normalizedMethod, "truck", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "Customer postal code is required for truck routes." });
            }

            try
            {
                var route = await _outboundDistribution.CalculateOutboundRoute(customerPostalCode, method, countryCode);
                float? loggedCarbon = null;

                if (!string.IsNullOrWhiteSpace(orderID))
                {
                    await _outboundDistribution.LogCalculatedRoute(orderID, route);
                    await _transportCarbonManager.SaveShippingMethod(orderID, route.SelectedMethod);
                    loggedCarbon = await _transportCarbonManager.CalculateCustomerDistanceCarbon(route.TotalDistanceKm, route.SelectedMethod);
                    await _transportCarbonManager.LogCustomerEmission(orderID, loggedCarbon.Value);
                }

                return Ok(new
                {
                    orderID,
                    companyName = OutboundDistribution.CompanyLocationName,
                    companyPostalCode = OutboundDistribution.CompanyPostalCode,
                    customerPostalCode,
                    countryCode,
                    shippingMethod = route.SelectedMethod,
                    distanceKm = Math.Round(route.TotalDistanceKm, 2),
                    durationMin = Math.Round(route.TotalDurationMin, 2),
                    routeChain = route.Formula,
                    formula = route.Formula,
                    loggedTransportCarbon = loggedCarbon.HasValue ? Math.Round((double)loggedCarbon.Value, 4) : (double?)null,
                    legs = route.Legs.Select(leg => new
                    {
                        method = leg.Method,
                        source = leg.Source,
                        destination = leg.Destination,
                        distanceKm = Math.Round(leg.DistanceKm, 2),
                        durationMin = Math.Round(leg.DurationMin, 2),
                        sourceLatitude = leg.SourceLatitude == null ? (double?)null : Math.Round(leg.SourceLatitude.Value, 6),
                        sourceLongitude = leg.SourceLongitude == null ? (double?)null : Math.Round(leg.SourceLongitude.Value, 6),
                        destinationLatitude = leg.DestinationLatitude == null ? (double?)null : Math.Round(leg.DestinationLatitude.Value, 6),
                        destinationLongitude = leg.DestinationLongitude == null ? (double?)null : Math.Round(leg.DestinationLongitude.Value, 6),
                        calculationType = leg.SourceType
                    })
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetCarbonLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var logs = await _carbonDataAggregator.GetCarbonLogs(page, pageSize);
            return Ok(logs);
        }

        private static string NormalizeShippingMethod(string method)
        {
            var normalized = (method ?? "truck").Trim().ToLowerInvariant();
            return normalized switch
            {
                "plane" => "air",
                "train" => "rail",
                _ => normalized
            };
        }

        private static bool IsMalaysiaDestination(string countryCode)
        {
            var normalized = (countryCode ?? string.Empty).Trim().ToLowerInvariant();
            return normalized is "my" or "mys" or "malaysia";
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
