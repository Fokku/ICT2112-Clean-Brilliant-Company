using CleanBrilliant.Models;

namespace CleanBrilliant.Services
{
    // The Interface for the Builder
    public interface IWidgetBuilder
    {
        IWidgetBuilder SetWidgetId(int id);
        IWidgetBuilder SetWidgetTitle(string title);
        IWidgetBuilder SetWidgetType(WidgetType type);
        IWidgetBuilder SetAggregationType(AggregationType agg);
        IWidgetBuilder SetComponentType(ComponentType comp);
        IWidgetBuilder SetThresholdValue(double threshold);
        Widget Build();
    }

    // The Concrete Builder Implementation
    public class WidgetBuilder : IWidgetBuilder
    {
        private Widget _widget;

        public WidgetBuilder()
        {
            // Always start with a fresh object
            _widget = new Widget() { WidgetTitle = "" };
        }

        public IWidgetBuilder SetWidgetId(int id)
        {
            _widget.WidgetId = id;
            return this;
        }

        public IWidgetBuilder SetWidgetTitle(string title)
        {
            _widget.WidgetTitle = title;
            return this;
        }

        public IWidgetBuilder SetWidgetType(WidgetType type)
        {
            _widget.Type = type;
            
            switch (type) {
                case WidgetType.PIE_2X2:
                case WidgetType.BAR_2X2:
                    _widget.DefaultWidth = 6; 
                    break;
                case WidgetType.BAR_4X2:
                    _widget.DefaultWidth = 12; 
                    break;
                case WidgetType.STAT:
                    _widget.DefaultWidth = 3; 
                    break;
            }
            return this;
        }

        public IWidgetBuilder SetComponentType(ComponentType comp)
        {
            _widget.Component = comp;
            return this;
        }

        public IWidgetBuilder SetAggregationType(AggregationType agg)
        {
            _widget.Aggregation = agg;
            return this;
        }

        public IWidgetBuilder SetThresholdValue(double threshold)
        {
            _widget.ThresholdValue = threshold;
            return this;
        }

        public Widget Build()
        {
            Widget completedWidget = _widget;
            _widget = new Widget() { WidgetTitle = "" }; // Reset for the next use
            return completedWidget;
        }
    }
}