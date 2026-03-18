using CleanBrilliant.Domain.Control;
using Microsoft.AspNetCore.Mvc;

namespace CleanBrilliant.Controllers
{
    [ApiController]
    [Route("api/carbon-analysis")]
    public class CarbonAnalysisController : ControllerBase
    {
        private readonly CarbonAnalysis _carbonAnalysis;

        public CarbonAnalysisController(CarbonAnalysis carbonAnalysis)
        {
            _carbonAnalysis = carbonAnalysis;
        }

        [HttpGet("recommend")]
        public async Task<IActionResult> GetShippingRecommendation(
            [FromQuery] string postalCode = "",
            [FromQuery] string deliveryType = "",
            [FromQuery] string countryCode = "")
        {
            var recommendation = await _carbonAnalysis.GetShippingRecommendation(postalCode, deliveryType, countryCode);
            return Ok(new { recommendation });
        }

        [HttpGet("analyze")]
        public IActionResult Analyze([FromQuery] float totalCarbon)
        {
            var level = _carbonAnalysis.Analyze(totalCarbon);
            return Ok(new { totalCarbon, level });
        }

        [HttpGet("estimate")]
        public IActionResult EstimateShippingCarbon(
            [FromQuery] string method = "truck",
            [FromQuery] float distanceKm = 0)
        {
            var estimate = _carbonAnalysis.EstimateShippingCarbon(method, distanceKm);
            return Ok(new { method, distanceKm, estimatedCarbon = estimate });
        }
    }
}
