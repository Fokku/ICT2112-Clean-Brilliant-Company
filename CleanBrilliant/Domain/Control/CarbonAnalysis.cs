using CleanBrilliant.Domain.DomainInterface;

namespace CleanBrilliant.Domain.Control
{
    public class CarbonAnalysis : ICarbonAnalysisService
    {
        private static readonly IReadOnlyDictionary<string, float> MalaysiaMethodDistances =
            new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
            {
                ["truck"] = 350f,
                ["air"] = 299f,
                ["ship"] = 326f,
                ["rail"] = 349f
            };

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

        public string RecommendLowestShippingMethod(float distanceKm, string deliveryType)
        {
            string best = "truck"; // Fallback default
            float lowest = float.MaxValue;

            // Map delivery speeds to capable shipping methods
            // 1 = Priority (Fastest), 2 = Express, 3 = Standard (Slowest)
            string[] allowedMethods = deliveryType switch
            {
                "1" => new[] { "air" },                           // Priority delivery is restricted to air
                "2" => new[] { "air", "truck" },                  // Express can be air or fast trucking
                "3" => new[] { "truck", "rail", "ship" },         // Standard allows for slower, low-carbon methods
                _ => new[] { "truck", "air", "ship", "rail" }     // Default fallback if input is empty/invalid
            };

            // Only evaluate the carbon footprint of methods allowed for this speed
            foreach (var method in allowedMethods)
            {
                float carbon = distanceKm * GetConfiguredCoefficient(method);
                if (carbon < lowest) 
                { 
                    lowest = carbon; 
                    best = method; 
                }
            }
            
            return best;
        }

        public async Task<string> GetShippingRecommendation(string postalCode, string deliveryType, string countryCode)
        {
            if (IsSingaporeDestination(countryCode))
            {
                return await Task.FromResult("truck");
            }

            if (IsMalaysiaDestination(countryCode))
            {
                return await Task.FromResult(RecommendLowestShippingMethodForMalaysia(deliveryType));
            }

            return await Task.FromResult("truck");
        }

        private string RecommendLowestShippingMethodForMalaysia(string deliveryType)
        {
            string best = "truck";
            float lowest = float.MaxValue;

            string[] allowedMethods = deliveryType switch
            {
                "1" => ["air"],
                "2" => ["air", "truck"],
                "3" => ["truck", "rail", "ship"],
                _ => ["truck", "air", "ship", "rail"]
            };

            foreach (var method in allowedMethods)
            {
                var distance = MalaysiaMethodDistances.TryGetValue(method, out var fixedDistance)
                    ? fixedDistance
                    : 350f;
                float carbon = distance * GetConfiguredCoefficient(method);
                if (carbon < lowest)
                {
                    lowest = carbon;
                    best = method;
                }
            }

            return best;
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

        private static bool IsSingaporeDestination(string countryCode)
        {
            var normalized = (countryCode ?? string.Empty).Trim().ToLowerInvariant();
            return normalized is "" or "sg" or "sgp" or "singapore";
        }

        private static bool IsMalaysiaDestination(string countryCode)
        {
            var normalized = (countryCode ?? string.Empty).Trim().ToLowerInvariant();
            return normalized is "my" or "mys" or "malaysia";
        }
    }
}
