using CleanBrilliant.Models;
using CleanBrilliant.Data.Gateways;

namespace CleanBrilliant.Services
{
    public class DashboardLayoutControl
    {
        private readonly DashboardLayoutGateway _dbGateway;
        private readonly DashboardLayoutSerializer _serializer;

        public DashboardLayoutControl(DashboardLayoutGateway dbGateway, DashboardLayoutSerializer serializer) 
        {
            _dbGateway = dbGateway;
            _serializer = serializer;
        }

        public void EnsureDefaultLayoutExists()
        {
            if (!_dbGateway.GetAllLayoutsJson().Any()) {
                var defaultLayout = new DashboardLayout { LayoutId = 1, LayoutName = "Default View", IsDefault = true };
                SaveLayout(defaultLayout);
            }
        }

        public List<DashboardLayout> GetAllLayouts()
        {
            var jsonList = _dbGateway.GetAllLayoutsJson();
            return _serializer.DeserializeList(jsonList);
        }

        public DashboardLayout GetLayout(int layoutId)
        {
            string jsonBlob = _dbGateway.GetDashboardLayoutJson(layoutId);
            return _serializer.Deserialize(jsonBlob);
        }

        public void SaveLayout(DashboardLayout layout)
        {
            string jsonBlob = _serializer.Serialize(layout);
            _dbGateway.UpdateDashboardLayout(layout.LayoutId, layout.LayoutName, layout.IsDefault, jsonBlob);
        }

        public void CreateNewLayout(string name)
        {
            int newId = GetAllLayouts().Any() ? GetAllLayouts().Max(l => l.LayoutId) + 1 : 1;
            SaveLayout(new DashboardLayout { LayoutId = newId, LayoutName = name });
        }

        // ORCHESTRATION METHODS
        public void AddWidgetToLayout(int layoutId, Widget newWidget)
        {
            var layout = GetLayout(layoutId);
            layout.AddPlacement(newWidget); // Entity does the math
            SaveLayout(layout); // Save back to DB
        }

        public void RemoveWidgetFromLayout(int layoutId, int placementId)
        {
            var layout = GetLayout(layoutId);
            layout.RemovePlacement(placementId); // Entity does the math
            SaveLayout(layout);
        }

        public void ReorderAndPackWidgets(int layoutId, List<int> orderedPlacementIds)
        {
            var layout = GetLayout(layoutId);
            layout.RecalculateGridPacking(orderedPlacementIds); // Entity does the math
            SaveLayout(layout);
        }
        
        public string GetRawJson(int layoutId)
        {
            return _dbGateway.GetDashboardLayoutJson(layoutId);
        }
    }
}