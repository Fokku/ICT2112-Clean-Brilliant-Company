using Microsoft.AspNetCore.Mvc;
using CleanBrilliant.Models;
using CleanBrilliant.Services;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace CleanBrilliant.Controllers
{
    public class DashboardController : Controller
    {
        private const string SessionStartDateKey = "Dashboard.StartDate";
        private const string SessionEndDateKey = "Dashboard.EndDate";

        private readonly DashboardLayoutControl _layoutControl;
        private readonly WidgetControl _widgetControl;
        private readonly CoefficientControl _coefficientControl;

        public DashboardController(
            DashboardLayoutControl layoutControl, 
            WidgetControl widgetControl,
            CoefficientControl coefficientControl)
        {
            _layoutControl = layoutControl;
            _widgetControl = widgetControl;
            _coefficientControl = coefficientControl;
        }

        [HttpGet("/dashboard")]
        public async Task<IActionResult> Index(int? layoutId, DateOnly? startDate, DateOnly? endDate)
        {
            // Now we ask the Control to get the layouts, instead of the Repo
            var layouts = await _layoutControl.GetAllLayouts();
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
            ViewBag.RawJson = JsonSerializer.Serialize(currentLayout.Placements);
            ViewBag.Coefficients = _coefficientControl.getAll();

            return View(currentLayout);
        }

        [HttpPost]
        public IActionResult UpdateCoefficient(string name, float emission)
        {
            try
            {
                _coefficientControl.updateEmission(name, emission);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Unexpected error occurred.");
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateLayout(string layoutName)
        {
            await _layoutControl.CreateNewLayout(layoutName);
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> AddWidget(int layoutId, string title, WidgetType type, AggregationType? agg, ComponentType comp, double? threshold, DateOnly? startDate, DateOnly? endDate)
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
            
            await _layoutControl.AddWidgetToLayout(layoutId, newWidget);
            return RedirectToAction("Index", "Dashboard", new { layoutId = layoutId, startDate = startDate, endDate = endDate });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveWidget(int layoutId, int placementId, DateOnly? startDate, DateOnly? endDate)
        {
            await _layoutControl.RemoveWidgetFromLayout(layoutId, placementId);
            return RedirectToAction("Index", "Dashboard", new { layoutId = layoutId, startDate = startDate, endDate = endDate });
        }

        // Class mapped for parsing JSON from frontend Drag-and-Drop
        public class ReorderRequest {
            public int LayoutId { get; set; }
            public required List<int> PlacementIds { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ReorderWidgets([FromBody] ReorderRequest req)
        {
            await _layoutControl.ReorderAndPackWidgets(req.LayoutId, req.PlacementIds);
            return Ok(); // Tells the frontend the save was successful
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLayout(int layoutId, DateOnly? startDate, DateOnly? endDate)
        {
            bool deleted = await _layoutControl.DeleteLayout(layoutId);
            if (!deleted)
            {
                TempData["ErrorMessage"] = "Cannot delete the default layout or layout not found.";
            }

            // Redirect to default layout after deletion
            return RedirectToAction("Index", "Dashboard", new { layoutId = (int?)null, startDate = startDate, endDate = endDate });
        }
    }
}