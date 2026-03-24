using CleanBrilliant.Models;

namespace CleanBrilliant.Services
{
    public class WidgetControl
    {
        private readonly IWidgetBuilder _builder;
        private readonly IAggregatedData _aggregatedData;

        public WidgetControl(IWidgetBuilder builder, IAggregatedData aggregatedData)
        {
            _builder = builder;
            _aggregatedData = aggregatedData;
        }

        // STAT widgets (no threshold)
        public Widget CreateStatWidget(int widgetId, string title, AggregationType agg, ComponentType comp)
        {
            return _builder.SetWidgetId(widgetId)
                           .SetWidgetTitle(title)
                           .SetWidgetType(WidgetType.STAT)
                           .SetAggregationType(agg)
                           .SetComponentType(comp)
                           .Build();
        }

        // Complex chart widgets (all params needed)
        public Widget CreateChartWidget(int widgetId, string title, WidgetType type, AggregationType agg, ComponentType comp, double threshold)
        {
            return _builder.SetWidgetId(widgetId)
                           .SetWidgetTitle(title)
                           .SetWidgetType(type)
                           .SetAggregationType(agg)
                           .SetComponentType(comp)
                           .SetThresholdValue(threshold)
                           .Build();
        }

        public List<CO2AggregatePoint> GetGraphData(Widget widget, DateOnly startDate, DateOnly endDate)
        {
            return _aggregatedData.GetAggregatedData(widget.Component, widget.Aggregation, startDate, endDate);
        }
    }
}