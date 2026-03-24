using System.Text.Json;
using CleanBrilliant.Models;

namespace CleanBrilliant.Services
{
    public class DashboardLayoutSerializer
    {
        public string Serialize(DashboardLayout layout)
        {
            return JsonSerializer.Serialize(layout);
        }

        public DashboardLayout Deserialize(string jsonBlob)
        {
            if (string.IsNullOrWhiteSpace(jsonBlob))
                throw new ArgumentException("Layout JSON cannot be null or empty.", nameof(jsonBlob));

            return JsonSerializer.Deserialize<DashboardLayout>(jsonBlob)
                ?? throw new JsonException("Failed to deserialize DashboardLayout.");
        }

        public List<DashboardLayout> DeserializeList(List<string> jsonBlobs)
        {
            var layouts = new List<DashboardLayout>();
            foreach (var blob in jsonBlobs)
            {
                layouts.Add(Deserialize(blob));
            }
            return layouts;
        }
    }
}