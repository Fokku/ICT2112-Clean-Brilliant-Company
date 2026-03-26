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

        public CarbonCalculationController(
            TransportCarbonManager transportCarbonManager,
            CarbonDataAggregator carbonDataAggregator)
        {
            _transportCarbonManager = transportCarbonManager;
            _carbonDataAggregator = carbonDataAggregator;
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

        [HttpGet("logs")]
        public async Task<IActionResult> GetCarbonLogs()
        {
            var logs = await _carbonDataAggregator.GetCarbonLogs();
            return Ok(logs);
        }
    }
}
