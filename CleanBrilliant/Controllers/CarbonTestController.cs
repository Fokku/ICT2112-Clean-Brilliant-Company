using CleanBrilliant.Data.Interfaces;
using CleanBrilliant.DTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace CleanBrilliant.Controllers
{
    public class CarbonTestController : Controller
    {
        private readonly IPreShipmentCarbonWriter _writer;
        private readonly IPreShipmentCarbonReader _reader;

        public CarbonTestController(IPreShipmentCarbonWriter writer, IPreShipmentCarbonReader reader)
        {
            _writer = writer;
            _reader = reader;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult InjectDummyData(int orderId, DateTime timeStamp, string productQuantities, int packageQty, string receivedDates)
        {
            try
            {
                var dto = new PreShipmentDetailDTO
                {
                    OrderId = orderId,
                    TimeStamp = timeStamp,
                    PackageQuantity = packageQty,
                    ProductOrderDetailList = new List<ProductOrderDetailDTO>(),
                    ReceivedDateList = new List<DateTime>()
                };

                // Parse Product Quantities (e.g., "10, 5, 2") into the DTO List
                if (!string.IsNullOrWhiteSpace(productQuantities))
                {
                    var qs = productQuantities.Split(',');
                    int pId = 1;
                    foreach (var q in qs)
                    {
                        if (int.TryParse(q.Trim(), out int qty))
                        {
                            dto.ProductOrderDetailList.Add(new ProductOrderDetailDTO { ProductID = pId++, Quantity = qty });
                        }
                    }
                }

                // Parse Received Dates (e.g., "2026-03-01, 2026-03-05") into the DTO List
                if (!string.IsNullOrWhiteSpace(receivedDates))
                {
                    var ds = receivedDates.Split(',');
                    foreach (var d in ds)
                    {
                        if (DateTime.TryParse(d.Trim(), out DateTime date))
                        {
                            dto.ReceivedDateList.Add(date);
                        }
                    }
                }

                // Write to database (Calculations happen here via your Calculator)
                _writer.CreatePreShipmentCarbonData(dto);

                // Instantly read it back to display the calculated math
                var savedData = _reader.GetPreShipmentCarbonData(orderId);

                if (savedData != null)
                {
                    TempData["SuccessMsg"] = $"Order {orderId} processed successfully!";
                    TempData["OutputResults"] = 
                        $"[CALCULATION OUTPUT]\n" +
                        $"Product CF: {savedData.ProductCF} kg\n" +
                        $"Storage CF: {savedData.StorageCF} kg\n" +
                        $"Packaging CF: {savedData.PackagingCF} kg\n" +
                        $"TOTAL CF: {savedData.TotalCF} kg";
                }
                else
                {
                    TempData["ErrorMsg"] = $"Order {orderId} was processed but couldn't be retrieved.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = $"Error saving data: {ex.Message}";
            }
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult FetchDummyData(int orderId)
        {
            try
            {
                var data = _reader.GetPreShipmentCarbonData(orderId);
                if (data == null)
                {
                    TempData["ErrorMsg"] = $"Order {orderId} not found.";
                    return RedirectToAction("Index");
                }

                TempData["FetchResult"] = $"Order {data.OrderId} | Time: {data.TimeStamp:yyyy-MM-dd} | Prod CF: {data.ProductCF} | Pkg CF: {data.PackagingCF} | Store CF: {data.StorageCF} | TOTAL: {data.TotalCF}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = $"Error fetching data: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}