using CleanBrilliant.Models;
using CleanBrilliant.Interfaces;
using Npgsql;

namespace CleanBrilliant.Services
{
    public class DashboardLayoutControl
    {
        private readonly IDashboardLayoutGateway _dbGateway;
        private readonly DashboardLayoutSerializer _serializer;

        public DashboardLayoutControl(IDashboardLayoutGateway dbGateway, DashboardLayoutSerializer serializer) 
        {
            _dbGateway = dbGateway;
            _serializer = serializer;
        }

        // =========================================================
        // 1. Core Lifecycle & Mapping Methods
        // =========================================================

        public async Task<List<DashboardLayout>> GetAllLayouts()
        {
            var layouts = new List<DashboardLayout>();
            
            // Get raw database reader from gateway
            using NpgsqlDataReader reader = await _dbGateway.GetAll();

            // Loop through the rows and map to objects
            while (await reader.ReadAsync())
            {
                var layout = new DashboardLayout
                {
                    LayoutId = reader.GetInt32(reader.GetOrdinal("layoutId")),
                    LayoutName = reader.GetString(reader.GetOrdinal("layoutName")),
                    IsDefault = reader.GetBoolean(reader.GetOrdinal("isDefault"))
                };

                // Safely read the JSON blob if it exists
                int jsonOrdinal = reader.GetOrdinal("gridWidgetConfig");
                if (!reader.IsDBNull(jsonOrdinal))
                {
                    string jsonBlob = reader.GetString(jsonOrdinal);
                    // The blob contains ONLY the Placements list
                    layout.Placements = _serializer.DeserializePlacements(jsonBlob);
                }

                layouts.Add(layout);
            }
            return layouts;
        }

        public async Task<DashboardLayout> GetLayout(int layoutId)
        {
            using NpgsqlDataReader reader = await _dbGateway.GetById(layoutId);

            if (await reader.ReadAsync())
            {
                var layout = new DashboardLayout
                {
                    LayoutId = reader.GetInt32(reader.GetOrdinal("layoutId")),
                    LayoutName = reader.GetString(reader.GetOrdinal("layoutName")),
                    IsDefault = reader.GetBoolean(reader.GetOrdinal("isDefault"))
                };

                int jsonOrdinal = reader.GetOrdinal("gridWidgetConfig");
                if (!reader.IsDBNull(jsonOrdinal))
                {
                    string jsonBlob = reader.GetString(jsonOrdinal);
                    layout.Placements = _serializer.DeserializePlacements(jsonBlob);
                }
                
                return layout;
            }
            return null; // Layout not found
        }

        public async Task SaveLayout(DashboardLayout layout)
        {
            // 1. Serialize ONLY the Placements list to JSON
            string jsonBlob = _serializer.SerializePlacements(layout.Placements);
            
            // 2. Pass the raw string to the gateway
            await _dbGateway.SaveOrUpdate(layout.LayoutId, layout.LayoutName, layout.IsDefault, jsonBlob);
        }

        public async Task EnsureDefaultLayoutExists()
        {
            if (!await _dbGateway.AnyLayoutsExist())
            {
                var defaultLayout = new DashboardLayout { LayoutId = 1, LayoutName = "Default View", IsDefault = true };
                await SaveLayout(defaultLayout);
            }
        }

        public async Task CreateNewLayout(string name)
        {
            var currentLayouts = await GetAllLayouts();
            int newId = currentLayouts.Any() ? currentLayouts.Max(l => l.LayoutId) + 1 : 1;
            
            await SaveLayout(new DashboardLayout { LayoutId = newId, LayoutName = name });
        }


        // =========================================================
        // 2. Orchestration Methods (Widget Management)
        // =========================================================

        public async Task AddWidgetToLayout(int layoutId, Widget newWidget)
        {
            // MUST use await here to get the actual object!
            var layout = await GetLayout(layoutId);
            if (layout == null) return;

            layout.AddPlacement(newWidget); // Entity does the math
            await SaveLayout(layout);       // Save back to DB
        }

        public async Task RemoveWidgetFromLayout(int layoutId, int placementId)
        {
            var layout = await GetLayout(layoutId);
            if (layout == null) return;

            layout.RemovePlacement(placementId); // Entity does the math
            await SaveLayout(layout);
        }

        public async Task ReorderAndPackWidgets(int layoutId, List<int> orderedPlacementIds)
        {
            var layout = await GetLayout(layoutId);
            if (layout == null) return;

            layout.RecalculateGridPacking(orderedPlacementIds); // Entity does the math
            await SaveLayout(layout);
        }

        public async Task<bool> DeleteLayout(int layoutId)
        {
            var layout = await GetLayout(layoutId);
            if (layout == null || layout.IsDefault)
            {
                return false; // Cannot delete non-existent or default layout
            }

            await _dbGateway.DeleteLayout(layoutId);
            return true;
        }
        
        // This handles the raw JSON display for your UI debugging
        public async Task<string> GetRawJson(int layoutId)
        {
            using NpgsqlDataReader reader = await _dbGateway.GetById(layoutId);
            if (await reader.ReadAsync())
            {
                int jsonOrdinal = reader.GetOrdinal("gridWidgetConfig");
                if (!reader.IsDBNull(jsonOrdinal))
                {
                    return reader.GetString(jsonOrdinal);
                }
            }
            return "{}"; // Return empty JSON object if null
        }
    }
}