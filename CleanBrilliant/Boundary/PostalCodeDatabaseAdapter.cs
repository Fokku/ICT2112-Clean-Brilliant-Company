using CleanBrilliant.Domain.DomainInterface;
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
            var url = "https://www.onemap.gov.sg/api/common/elastic/search"
                + $"?searchVal={Uri.EscapeDataString(postalCode)}"
                + "&returnGeom=Y&getAddrDetails=N&pageNum=1";

            try
            {
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
            catch
            {
                return null;
            }
        }
    }
}
