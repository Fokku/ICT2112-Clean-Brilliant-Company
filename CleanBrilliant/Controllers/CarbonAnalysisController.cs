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
            [FromQuery] string countryCode = "",
            [FromQuery] string strategy = "rules")
        {
            if (string.Equals(strategy, "duration", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _carbonAnalysis.GetShippingRecommendationByDuration(postalCode, deliveryType, countryCode);
                return Ok(new
                {
                    recommendation = result.Recommendation,
                    strategy = result.Strategy,
                    deliveryType = result.DeliveryType,
                    candidates = result.Candidates.Select(candidate => new
                    {
                        method = candidate.Method,
                        distanceKm = candidate.DistanceKm,
                        durationMin = candidate.DurationMin,
                        averageSpeedKmPerHour = candidate.AverageSpeedKmPerHour,
                        estimatedCarbon = candidate.EstimatedCarbon,
                        routeChain = candidate.RouteChain
                    })
                });
            }

            var recommendation = await _carbonAnalysis.GetShippingRecommendation(postalCode, deliveryType, countryCode);
            return Ok(new { recommendation, strategy = "rules" });
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
