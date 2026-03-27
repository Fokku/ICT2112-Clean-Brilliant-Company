using System;

namespace CleanBrilliant.Interfaces
{
    public interface IWidgetManager
    {
        void displayWidgetConfig();
        void populateWidget(int widgetId, DateOnly startDate, DateOnly endDate);
    }
}