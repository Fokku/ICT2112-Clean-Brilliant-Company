using CleanBrilliant.Domain.DomainInterface;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace CleanBrilliant.Boundary
{
    public class PostalCodeDatabaseAdapter : IPostalService
    {
        private readonly HttpClient _httpClient;

        public PostalCodeDatabaseAdapter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(double Latitude, double Longitude)?> GetCoordinates(string postalCode)
        {
            try
            {
                if (IsSingaporeQuery(postalCode))
                {
                    return await GetSingaporeCoordinates(postalCode);
                }

                return await GetNominatimCoordinates(postalCode);
            }
            catch
            {
                return null;
            }
        }

        private async Task<(double Latitude, double Longitude)?> GetSingaporeCoordinates(string query)
        {
            var url = "https://www.onemap.gov.sg/api/common/elastic/search"
                + $"?searchVal={Uri.EscapeDataString(query)}"
                + "&returnGeom=Y&getAddrDetails=N&pageNum=1";

            using var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);
            if (!document.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
            {
                return null;
            }

            var firstResult = results[0];
            if (!firstResult.TryGetProperty("LATITUDE", out var latitudeElement) ||
                !firstResult.TryGetProperty("LONGITUDE", out var longitudeElement))
            {
                return null;
            }

            if (!double.TryParse(latitudeElement.GetString(), out var latitude) ||
                !double.TryParse(longitudeElement.GetString(), out var longitude))
            {
                return null;
            }

            return (latitude, longitude);
        }

        private async Task<(double Latitude, double Longitude)?> GetNominatimCoordinates(string query)
        {
            foreach (var url in BuildNominatimUrls(query))
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("CleanBrilliant", "1.0"));

                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(stream);
                if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
                {
                    continue;
                }

                var firstResult = document.RootElement[0];
                if (!firstResult.TryGetProperty("lat", out var latitudeElement) ||
                    !firstResult.TryGetProperty("lon", out var longitudeElement))
                {
                    continue;
                }

                if (!double.TryParse(latitudeElement.GetString(), out var latitude) ||
                    !double.TryParse(longitudeElement.GetString(), out var longitude))
                {
                    continue;
                }

                return (latitude, longitude);
            }

            return null;
        }

        private static IEnumerable<string> BuildNominatimUrls(string query)
        {
            var trimmed = query.Trim();
            if (Regex.IsMatch(trimmed, @"^\d{5}$"))
            {
                yield return "https://nominatim.openstreetmap.org/search"
                    + $"?postalcode={Uri.EscapeDataString(trimmed)}"
                    + "&countrycodes=my&format=jsonv2&limit=1";

                yield return "https://nominatim.openstreetmap.org/search"
                    + $"?q={Uri.EscapeDataString(trimmed + ", Malaysia")}"
                    + "&countrycodes=my&format=jsonv2&limit=1";

                yield return "https://nominatim.openstreetmap.org/search"
                    + $"?q={Uri.EscapeDataString("postcode " + trimmed + ", Malaysia")}"
                    + "&countrycodes=my&format=jsonv2&limit=1";

                yield break;
            }

            yield return "https://nominatim.openstreetmap.org/search"
                + $"?q={Uri.EscapeDataString(trimmed)}"
                + "&format=jsonv2&limit=1";

            if (!trimmed.Contains("malaysia", StringComparison.OrdinalIgnoreCase))
            {
                yield return "https://nominatim.openstreetmap.org/search"
                    + $"?q={Uri.EscapeDataString(trimmed + ", Malaysia")}"
                    + "&countrycodes=my&format=jsonv2&limit=1";
            }
        }

        private static bool IsSingaporeQuery(string query)
        {
            var trimmed = query.Trim();
            return Regex.IsMatch(trimmed, @"^\d{6}$") ||
                   trimmed.Contains("singapore", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.Contains("punggol", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.Contains("jurong", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.Contains("changi", StringComparison.OrdinalIgnoreCase) ||
                   trimmed.Contains("woodlands", StringComparison.OrdinalIgnoreCase);
        }
    }
}
