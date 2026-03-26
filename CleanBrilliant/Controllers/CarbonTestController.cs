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
        public IActionResult InjectDummyData(int orderId, DateTime timeStamp, int qty101, int qty102, int? customProductId, int? customProductQty, int packageQty, string receivedDates, float? shipmentCf)
        {
            try
            {
                var dto = new PreShipmentDetailDTO
                {
                    OrderId = orderId,
                    TimeStamp = timeStamp,
                    PackageQuantity = packageQty,
                    ProductOrderDetailList = new List<ProductOrderDetailDTO>(),
                    ReceivedDateList = new List<DateTime>(),
                    ShipmentCF = shipmentCf // Pass it to the DTO
                };

                if (qty101 > 0) dto.ProductOrderDetailList.Add(new ProductOrderDetailDTO { ProductID = 101, Quantity = qty101 });
                if (qty102 > 0) dto.ProductOrderDetailList.Add(new ProductOrderDetailDTO { ProductID = 102, Quantity = qty102 });

                if (customProductId.HasValue && customProductQty.HasValue && customProductQty.Value > 0)
                {
                    dto.ProductOrderDetailList.Add(new ProductOrderDetailDTO { ProductID = customProductId.Value, Quantity = customProductQty.Value });
                }

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

                _writer.CreatePreShipmentCarbonData(dto);

                var savedData = _reader.GetPreShipmentCarbonData(orderId);

                if (savedData != null)
                {
                    TempData["SuccessMsg"] = $"Order {orderId} processed successfully!";
                    TempData["OutputResults"] = 
                        $"[CALCULATION OUTPUT]\n" +
                        $"Product CF: {savedData.ProductCF} kg\n" +
                        $"Storage CF: {savedData.StorageCF} kg\n" +
                        $"Packaging CF: {savedData.PackagingCF} kg\n" +
                        $"Shipment CF: {savedData.ShipmentCF} kg (Debug Override)\n" +
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

                TempData["FetchResult"] = $"Order {data.OrderId} | Time: {data.TimeStamp:yyyy-MM-dd} | Prod CF: {data.ProductCF} | Pkg CF: {data.PackagingCF} | Store CF: {data.StorageCF} | Ship CF: {data.ShipmentCF} | TOTAL: {data.TotalCF}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMsg"] = $"Error fetching data: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}