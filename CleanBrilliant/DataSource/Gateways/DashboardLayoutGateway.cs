namespace CleanBrilliant.Data.Gateways
{
    public class DashboardLayoutGateway
    {
        // Simulates your MySQL Database Table: LayoutId -> JSON Blob
        private static readonly Dictionary<int, string> _databaseTable = new();

        public string? GetDashboardLayoutJson(int layoutId)
        {
            if (_databaseTable.TryGetValue(layoutId, out string? jsonBlob))
                return jsonBlob;

            return null;
        }

        public List<string> GetAllLayoutsJson()
        {
            return _databaseTable.Values.ToList();
        }

        public void UpdateDashboardLayout(int layoutId, string layoutName, bool isDefault, string gridWidgetConfigJson)
        {
            // In a real app, this is an SQL INSERT/UPDATE statement
            _databaseTable[layoutId] = gridWidgetConfigJson;
        }
    }
}