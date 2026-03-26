using Microsoft.AspNetCore.Mvc;

namespace CleanBrilliant.Controllers
{
    [Route("Carbon")]
    public class CarbonController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Employees")]
        public IActionResult Employees()
        {
            return View();
        }

        [HttpGet("Buildings")]
        public IActionResult Buildings()
        {
            return View();
        }

        [HttpGet("Shipping")]
        public IActionResult Shipping()
        {
            return View();
        }

        [HttpGet("Analysis")]
        public IActionResult Analysis()
        {
            return View();
        }

        [HttpGet("RouteSummary")]
        public IActionResult RouteSummary()
        {
            return View();
        }

        [HttpGet("RouteCalculation")]
        public IActionResult RouteCalculation()
        {
            return View();
        }

        [HttpGet("CustomerHubRouteCalculation")]
        public IActionResult CustomerHubRouteCalculation()
        {
            return RedirectToAction(nameof(RouteCalculation));
        }

        [HttpGet("CustomerDistanceCalculation")]
        public IActionResult CustomerDistanceCalculation()
        {
            return RedirectToAction(nameof(RouteCalculation));
        }

        [HttpGet("HubLookup")]
        public IActionResult HubLookup()
        {
            return RedirectToAction(nameof(RouteCalculation));
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
