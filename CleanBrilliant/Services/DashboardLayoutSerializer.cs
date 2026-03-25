using System.Text.Json;
using CleanBrilliant.Models;

namespace CleanBrilliant.Services
{
    public class DashboardLayoutSerializer
    {
        // Notice we are passing List<GridPlacement> now
        public string SerializePlacements(List<GridPlacement> placements)
        {
            return JsonSerializer.Serialize(placements);
        }

        public List<GridPlacement> DeserializePlacements(string jsonBlob)
        {
            if (string.IsNullOrEmpty(jsonBlob)) return new List<GridPlacement>();
            return JsonSerializer.Deserialize<List<GridPlacement>>(jsonBlob);
        }
    }
}