namespace CleanBrilliant.Models
{
    public class Widget
    {
        public int WidgetId { get; set; }
        public required string WidgetTitle { get; set; }
        public WidgetType Type { get; set; }
        public AggregationType Aggregation { get; set; }
        public ComponentType Component { get; set; }
        public double ThresholdValue { get; set; }
        public int DefaultWidth { get; set; }
        public int DefaultHeight { get; set; }
    }
}