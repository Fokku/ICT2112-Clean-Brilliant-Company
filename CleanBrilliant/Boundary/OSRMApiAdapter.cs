using CleanBrilliant.Domain.DomainInterface;
using System.Text.Json;

namespace CleanBrilliant.Boundary
{
    public class OSRMApiAdapter : IOSRMService
    {
        private readonly HttpClient _httpClient;

        public OSRMApiAdapter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<float> GetRouteDistance(double longitude, double latitude)
        {
            var url = $"http://router.project-osrm.org/route/v1/driving/{longitude},{latitude};{longitude},{latitude}?overview=false";
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);
                var distance = doc.RootElement
                    .GetProperty("routes")[0]
                    .GetProperty("distance")
                    .GetSingle();
                return distance / 1000f; // meters to km
            }
            catch (Exception)
            {
                return 0f;
            }
        }
    }
}
