using CleanBrilliant.Domain.DomainInterface;

namespace CleanBrilliant.Domain.Control
{
    public class CarbonAnalysis : ICarbonAnalysisService
    {
        private readonly float _lowThreshold = 50.0f;
        private readonly float _highThreshold = 200.0f;
        private readonly IPostalService _postalService;
        private readonly IOSRMService _osrmService;

        private static readonly Dictionary<string, float> Coefficients = new()
        {
            { "air", 1.5f },
            { "truck", 0.5f },
            { "ship", 0.1f },
            { "rail", 0.05f }
        };

        public CarbonAnalysis(IPostalService postalService, IOSRMService osrmService)
        {
            _postalService = postalService;
            _osrmService = osrmService;
        }

        public string Analyze(float totalCarbon)
        {
            if (totalCarbon <= _lowThreshold) return "LOW";
            if (totalCarbon <= _highThreshold) return "MEDIUM";
            return "HIGH";
        }

        public float EstimateShippingCarbon(string method, float distanceKm)
        {
            if (Coefficients.TryGetValue(method.ToLower(), out float coeff))
                return distanceKm * coeff;
            return distanceKm * 0.5f; // default to truck
        }

        public string RecommendLowestShippingMethod(float distanceKm)
        {
            string best = "truck";
            float lowest = float.MaxValue;
            foreach (var kvp in Coefficients)
            {
                float carbon = distanceKm * kvp.Value;
                if (carbon < lowest) { lowest = carbon; best = kvp.Key; }
            }
            return best;
        }

        public async Task<string> GetShippingRecommendation(string postalCode, string deliveryType, string countryCode)
        {
            const string warehousePostal = "018960";
            var (source, dest) = await _postalService.GetPostalConversion(warehousePostal, postalCode);
            float distance = await _osrmService.GetRouteDistance(source.Longitude, source.Latitude, dest.Longitude, dest.Latitude);
            if (distance <= 0)
                distance = 100f;
            return RecommendLowestShippingMethod(distance);
        }
    }
}
