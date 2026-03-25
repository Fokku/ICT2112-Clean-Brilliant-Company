using CleanBrilliant.Domain.DomainInterface;

namespace CleanBrilliant.Domain.Control
{
    public class CarbonAnalysis : ICarbonAnalysisService
    {
        private readonly float _lowThreshold = 50.0f;
        private readonly float _highThreshold = 200.0f;

        private readonly CleanBrilliant.Services.ICoefficientManager _coefficientManager;

        public CarbonAnalysis(CleanBrilliant.Services.ICoefficientManager coefficientManager)
        {
            _coefficientManager = coefficientManager;
        }

        public string Analyze(float totalCarbon)
        {
            if (totalCarbon <= _lowThreshold) return "LOW";
            if (totalCarbon <= _highThreshold) return "MEDIUM";
            return "HIGH";
        }

        public float EstimateShippingCarbon(string method, float distanceKm)
        {
            var coeff = GetConfiguredCoefficient(method);
            return distanceKm * coeff;
        }

        public string RecommendLowestShippingMethod(float distanceKm)
        {
            string best = "truck";
            float lowest = float.MaxValue;
            foreach (var method in new[] { "truck", "air", "ship", "rail" })
            {
                float carbon = distanceKm * GetConfiguredCoefficient(method);
                if (carbon < lowest) { lowest = carbon; best = method; }
            }
            return best;
        }

        public async Task<string> GetShippingRecommendation(string postalCode, string deliveryType, string countryCode)
        {
            return await Task.FromResult(RecommendLowestShippingMethod(100)); // placeholder distance
        }

        private float GetConfiguredCoefficient(string method)
        {
            var normalized = NormalizeMethod(method);

            try
            {
                return _coefficientManager.getEmission(normalized);
            }
            catch (KeyNotFoundException)
            {
                return normalized switch
                {
                    "plane" => 1.5f,
                    "truck" => 0.5f,
                    "ship" => 0.1f,
                    "train" => 0.05f,
                    _ => 0.5f
                };
            }
        }

        private static string NormalizeMethod(string method)
        {
            var normalized = (method ?? "truck").Trim().ToLowerInvariant();
            return normalized switch
            {
                "air" => "plane",
                "rail" => "train",
                _ => normalized
            };
        }
    }
}
