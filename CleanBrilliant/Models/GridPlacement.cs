namespace CleanBrilliant.Models
{
    public class GridPlacement
    {
        public int PlacementId { get; set; }
        public int WidgetId { get; set; }
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }
        public required Widget Widget { get; set; } 
    }
}