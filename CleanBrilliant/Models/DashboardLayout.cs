namespace CleanBrilliant.Models
{
    public class DashboardLayout
    {
        public int LayoutId { get; set; }
        public required string LayoutName { get; set; }
        public bool IsDefault { get; set; }
        public List<GridPlacement> Placements { get; set; } = new List<GridPlacement>();

        // --- DOMAIN LOGIC (Grid Math) MOVED HERE ---
        public void AddPlacement(Widget widget)
        {
            var placement = new GridPlacement {
                PlacementId = new Random().Next(1000, 9999),
                WidgetId = widget.WidgetId,
                Widget = widget
            };
            Placements.Add(placement);
            RecalculateGridPacking(Placements.Select(p => p.PlacementId).ToList());
        }

        public void RemovePlacement(int placementId)
        {
            Placements.RemoveAll(p => p.PlacementId == placementId);
            RecalculateGridPacking(Placements.Select(p => p.PlacementId).ToList());
        }

        public void RecalculateGridPacking(List<int> orderedPlacementIds)
        {
            // Reorder based on UI drag-and-drop
            var reorderedList = new List<GridPlacement>();
            foreach (var id in orderedPlacementIds) {
                var p = Placements.FirstOrDefault(x => x.PlacementId == id);
                if (p != null) reorderedList.Add(p);
            }
            Placements = reorderedList;

            // First-Fit Algorithm (Fixes empty pockets)
            var rowRemainingSpace = new List<int>(); 

            foreach (var placement in Placements)
            {
                int widgetWidth = placement.Widget.DefaultWidth;
                bool isPlaced = false;

                for (int rowIndex = 0; rowIndex < rowRemainingSpace.Count; rowIndex++)
                {
                    if (rowRemainingSpace[rowIndex] >= widgetWidth)
                    {
                        placement.RowIndex = rowIndex;
                        placement.ColIndex = 12 - rowRemainingSpace[rowIndex]; 
                        rowRemainingSpace[rowIndex] -= widgetWidth; 
                        isPlaced = true;
                        break; 
                    }
                }

                if (!isPlaced)
                {
                    int newRowIndex = rowRemainingSpace.Count;
                    placement.RowIndex = newRowIndex;
                    placement.ColIndex = 0; 
                    rowRemainingSpace.Add(12 - widgetWidth); 
                }
            }
        }
    }
}