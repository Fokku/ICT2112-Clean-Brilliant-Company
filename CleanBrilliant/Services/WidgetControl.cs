using System;
using System.Collections.Generic;
using System.Text.Json;
using CleanBrilliant.Models;
using CleanBrilliant.Interfaces;

namespace CleanBrilliant.Services
{
    public class WidgetControl : IWidgetManager
    {
        private readonly IWidgetBuilder _builder;
        private readonly IAggregatedData _aggregatedData;
        private readonly ILayoutWidgetManager _layoutWidgetManager; // 1. Added Dependency
        
        private readonly Dictionary<int, string> _widgetDataCache = new();

        // 2. Injected ILayoutWidgetManager
        public WidgetControl(IWidgetBuilder builder, IAggregatedData aggregatedData, ILayoutWidgetManager layoutWidgetManager)
        {
            _builder = builder;
            _aggregatedData = aggregatedData;
            _layoutWidgetManager = layoutWidgetManager;
        }

        public Widget CreateStatWidget(int widgetId, string title, AggregationType agg, ComponentType comp)
        {
            return _builder.SetWidgetId(widgetId)
                           .SetWidgetTitle(title)
                           .SetWidgetType(WidgetType.STAT)
                           .SetAggregationType(agg)
                           .SetComponentType(comp)
                           .Build();
        }

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

        // 3. Added from UML diagram, satisfying the "uses" relationship
        public void removeWidget(int widgetId)
        {
            // Call the layout manager to remove it from the grid
            _layoutWidgetManager.removeWidgetPlacement(widgetId);
            
            // Remove from local state
            if (_widgetDataCache.ContainsKey(widgetId))
            {
                _widgetDataCache.Remove(widgetId);
            }
        }

        public void displayWidgetConfig()
        {
            var availableConfigs = new List<object>
            {
                new { Type = WidgetType.STAT, RequiresThreshold = false, DefaultWidth = 3 },
                new { Type = WidgetType.BAR_2X2, RequiresThreshold = true, DefaultWidth = 6 },
                new { Type = WidgetType.PIE_2X2, RequiresThreshold = true, DefaultWidth = 6 },
                new { Type = WidgetType.BAR_4X2, RequiresThreshold = true, DefaultWidth = 12 }
            };

            string serializedConfig = JsonSerializer.Serialize(availableConfigs, new JsonSerializerOptions { WriteIndented = true });
            System.Diagnostics.Debug.WriteLine($"[WidgetControl] Active Layout Config Parsed:\n{serializedConfig}");
        }

        public void populateWidget(int widgetId, DateOnly startDate, DateOnly endDate)
        {
            Widget mockRetrievedWidget = _builder
                .SetWidgetId(widgetId)
                .SetWidgetTitle($"Active Widget #{widgetId}")
                .SetWidgetType(WidgetType.BAR_2X2)
                .SetComponentType(ComponentType.TOTAL)
                .SetAggregationType(AggregationType.SUM)
                .SetThresholdValue(250.0) 
                .Build();

            var rawDataPoints = _aggregatedData.GetAggregatedData(
                mockRetrievedWidget.Component, 
                mockRetrievedWidget.Aggregation, 
                startDate, 
                endDate);

            int thresholdBreaches = 0;
            double totalEmissions = 0;

            foreach (var point in rawDataPoints)
            {
                totalEmissions += point.Value;
                if (point.Value > mockRetrievedWidget.ThresholdValue)
                {
                    thresholdBreaches++;
                }
            }

            var processedPayload = new
            {
                WidgetId = mockRetrievedWidget.WidgetId,
                Title = mockRetrievedWidget.WidgetTitle,
                TotalDataPoints = rawDataPoints.Count,
                AggregateTotal = totalEmissions,
                BreachCount = thresholdBreaches,
                Status = thresholdBreaches > 0 ? "Warning" : "Optimal",
                LastUpdated = DateTime.UtcNow
            };

            _widgetDataCache[widgetId] = JsonSerializer.Serialize(processedPayload);
            System.Diagnostics.Debug.WriteLine($"[WidgetControl] Populated Widget {widgetId}. Cached {rawDataPoints.Count} points. Breaches: {thresholdBreaches}");
        }
    }
}