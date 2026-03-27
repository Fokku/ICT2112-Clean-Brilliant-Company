using CleanBrilliant.Models;

namespace CleanBrilliant.Interfaces
{
    public interface ILayoutWidgetManager
    {
        void removeWidgetPlacement(int widgetId);
        void addWidgetToLayout(int layoutId, int widgetId, Widget widget, int rowIndex, int colIndex);
    }
}