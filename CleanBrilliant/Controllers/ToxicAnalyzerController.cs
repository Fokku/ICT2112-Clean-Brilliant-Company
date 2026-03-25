using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using CleanBrilliant.Interfaces;
using CleanBrilliant.Models;

namespace CleanBrilliant.Controllers
{
    public class ToxicAnalyzerController : Controller
    {
        private readonly IProductDetailWriter _writer;
        private readonly IProductDetailReader _reader;

        // Inject both interfaces
        public ToxicAnalyzerController(IProductDetailWriter writer, IProductDetailReader reader)
        {
            _writer = writer;
            _reader = reader;
        }

        // GET: /ToxicAnalyzer
        public IActionResult Index()
        {
            return View();
        }

        // POST: /ToxicAnalyzer/Calculate
        [HttpPost]
        public IActionResult Calculate(int productId, float totalVolume, List<Ingredient> ingredients)
        {
            ingredients.RemoveAll(i => string.IsNullOrWhiteSpace(i.Name));
            var result = _writer.CreateProductDetail(productId, totalVolume, ingredients);
            
            ViewBag.Result = result;
            return View("Index");
        }

        // GET: /ToxicAnalyzer/Read
        [HttpGet]
        public IActionResult Read(int searchProductId)
        {
            var result = _reader.GetProductDetail(searchProductId);
            
            if (result == null)
            {
                TempData["SearchError"] = $"No product found with ID: {searchProductId}";
            }
            
            // Reusing the same view, but passing the read result
            ViewBag.ReadResult = result;
            return View("Index");
        }
    }
}