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
            var carbon = await _transportCarbonManager.GetOrderShippingCarbon(orderID);
            return Ok(new { orderID, shippingCarbon = carbon });
        }

        [HttpGet("restock/{restockID}/shipping")]
        public async Task<IActionResult> GetRestockShippingCarbon([FromRoute] string restockID)
        {
            var carbon = await _transportCarbonManager.GetRestockShippingCarbon(restockID);
            return Ok(new { restockID, shippingCarbon = carbon });
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

        [HttpGet("logs")]
        public async Task<IActionResult> GetCarbonLogs()
        {
            var logs = await _carbonDataAggregator.GetCarbonLogs();
            return Ok(logs);
        }
    }
}
