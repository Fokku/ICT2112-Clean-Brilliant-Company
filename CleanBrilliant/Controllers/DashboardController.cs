using Microsoft.AspNetCore.Mvc;
using CleanBrilliant.Models;
using CleanBrilliant.Domain.Control;
using CleanBrilliant.Services;
using Microsoft.AspNetCore.Http;

namespace CleanBrilliant.Controllers
{
    public class DashboardController : Controller
    {
        private const string SessionStartDateKey = "Dashboard.StartDate";
        private const string SessionEndDateKey = "Dashboard.EndDate";

        private readonly DashboardLayoutControl _layoutControl;
        private readonly WidgetControl _widgetControl;

        public DashboardController(DashboardLayoutControl layoutControl, WidgetControl widgetControl)
        {
            _layoutControl = layoutControl;
            _widgetControl = widgetControl;
            _layoutControl.EnsureDefaultLayoutExists(); // Make sure we have 1 layout on startup
        }

        [HttpGet("/dashboard")]
        public IActionResult Index(int? layoutId, DateOnly? startDate, DateOnly? endDate)
        {
            // Now we ask the Control to get the layouts, instead of the Repo
            var layouts = _layoutControl.GetAllLayouts();
            var currentLayout = layoutId.HasValue ? layouts.FirstOrDefault(l => l.LayoutId == layoutId) : layouts.FirstOrDefault();
            if (currentLayout == null)
            {
                throw new InvalidOperationException("No dashboard layouts were found.");
            }

            if (!startDate.HasValue)
            {
                var sessionStartDate = HttpContext.Session.GetString(SessionStartDateKey);
                if (DateOnly.TryParse(sessionStartDate, out var parsedStartDate))
                {
                    startDate = parsedStartDate;
                }
            }

            if (!endDate.HasValue)
            {
                var sessionEndDate = HttpContext.Session.GetString(SessionEndDateKey);
                if (DateOnly.TryParse(sessionEndDate, out var parsedEndDate))
                {
                    endDate = parsedEndDate;
                }
            }

            var resolvedStartDate = startDate ?? new DateOnly(2026, 1, 1);
            var resolvedEndDate = endDate ?? DateOnly.FromDateTime(DateTime.Today);
            if (resolvedStartDate > resolvedEndDate)
            {
                (resolvedStartDate, resolvedEndDate) = (resolvedEndDate, resolvedStartDate);
            }

            HttpContext.Session.SetString(SessionStartDateKey, resolvedStartDate.ToString("yyyy-MM-dd"));
            HttpContext.Session.SetString(SessionEndDateKey, resolvedEndDate.ToString("yyyy-MM-dd"));

            ViewBag.AllLayouts = layouts;
            ViewBag.CurrentLayoutId = currentLayout.LayoutId;
            ViewBag.StartDate = resolvedStartDate;
            ViewBag.EndDate = resolvedEndDate;
            ViewBag.WidgetControl = _widgetControl; 
            ViewBag.RawJson = _layoutControl.GetRawJson(currentLayout.LayoutId); 

            return View(currentLayout);
        }

        [HttpPost]
        public IActionResult CreateLayout(string layoutName)
        {
            _layoutControl.CreateNewLayout(layoutName);
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public IActionResult AddWidget(int layoutId, string title, WidgetType type, AggregationType? agg, ComponentType comp, double? threshold, DateOnly? startDate, DateOnly? endDate)
        {
            int newWidgetId = new Random().Next(1000, 9999);
            Widget newWidget;
            var resolvedAgg = agg ?? AggregationType.SUM;
            var resolvedThreshold = threshold ?? 50;

            if (type == WidgetType.STAT) 
            {
                newWidget = _widgetControl.CreateStatWidget(newWidgetId, title, resolvedAgg, comp);
            }
            else 
            {

                newWidget = _widgetControl.CreateChartWidget(newWidgetId, title, type, resolvedAgg, comp, resolvedThreshold);
            }
            
            _layoutControl.AddWidgetToLayout(layoutId, newWidget);
            return RedirectToAction("Index", "Dashboard", new { layoutId = layoutId, startDate = startDate, endDate = endDate });
        }

        [HttpPost]
        public IActionResult RemoveWidget(int layoutId, int placementId, DateOnly? startDate, DateOnly? endDate)
        {
            _layoutControl.RemoveWidgetFromLayout(layoutId, placementId);
            return RedirectToAction("Index", "Dashboard", new { layoutId = layoutId, startDate = startDate, endDate = endDate });
        }

        // Class mapped for parsing JSON from frontend Drag-and-Drop
        public class ReorderRequest {
            public int LayoutId { get; set; }
            public required List<int> PlacementIds { get; set; }
        }

        [HttpPost]
        public IActionResult ReorderWidgets([FromBody] ReorderRequest req)
        {
            _layoutControl.ReorderAndPackWidgets(req.LayoutId, req.PlacementIds);
            return Ok(); // Tells the frontend the save was successful
        }
    }
}