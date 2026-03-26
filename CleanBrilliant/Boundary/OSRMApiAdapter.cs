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

        public async Task<(float DistanceKm, float DurationMin)> GetRoute(
            double sourceLongitude,
            double sourceLatitude,
            double destinationLongitude,
            double destinationLatitude)
        {
            var url = "http://router.project-osrm.org/route/v1/driving/"
                + $"{sourceLongitude},{sourceLatitude};{destinationLongitude},{destinationLatitude}"
                + "?overview=false";
            try
            {
                using var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"OSRM request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).");
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                if (!doc.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
                {
                    throw new InvalidOperationException("OSRM returned no routes for the selected postal codes.");
                }

                var route = routes[0];
                if (!route.TryGetProperty("distance", out var distanceElement) ||
                    !route.TryGetProperty("duration", out var durationElement))
                {
                    throw new InvalidOperationException("OSRM response did not include distance and duration values.");
                }

                var distanceKm = distanceElement.GetSingle() / 1000f;
                var durationMin = durationElement.GetSingle() / 60f;
                return (distanceKm, durationMin);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException($"Failed to reach OSRM service: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException("OSRM request timed out.", ex);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse OSRM response.", ex);
            }
        }
    }
}
