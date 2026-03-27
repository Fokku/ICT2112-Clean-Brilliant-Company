using System.Collections.Generic;
using CleanBrilliant.Models;

namespace CleanBrilliant.Interfaces
{
    public interface ILayoutManager
    {
        DashboardLayout getLayout(int layoutId);
        void updateLayout(int layoutId, string name, string gridWidgetConfig);
        List<DashboardLayout> getLayoutList();
        void addWidgetToLayout(int layoutId, int widgetId, Widget widget, int rowIndex, int colIndex);
        void removeWidgetPlacement(int widgetId);
    }
}