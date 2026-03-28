using System;
using System.Collections.Generic;
using CleanBrilliant.Data; // Added to access RecordSet
using CleanBrilliant.Data.Interfaces;
using CleanBrilliant.Data.Gateways;
using CleanBrilliant.DTO;
using CleanBrilliant.Models.CarbonStrategies;
using CleanBrilliant.Models.Interfaces;
using CleanBrilliant.Interfaces;

namespace CleanBrilliant.Services
{
    public class PreShipmentCarbonCalculator : IPreShipmentCarbonReader, IPreShipmentCarbonWriter
    {
        private ICarbonFootprintStrategy _strategy;
        private readonly PreShipmentGateway _gateway;
        private readonly IProductDetailReader _productReader;
        private const string TableName = "pre_shipment_carbon_data";

        public PreShipmentCarbonCalculator(PreShipmentGateway gateway, IProductDetailReader productReader)
        {
            _strategy = new ProductCF();
            _gateway = gateway;
            _productReader = productReader;
        }

        public void SetStrategy(ICarbonFootprintStrategy strategy)
        {
            _strategy = strategy;
        }

        public float CalculateCarbon(PreShipmentDetailDTO dto)
        {
          

            // For PackagingCF and StorageCF, fall back to the normal strategy math
            return _strategy.CalculateCarbon(dto);
        }

        public PreShipmentCarbonDataDTO CalculateAll(PreShipmentDetailDTO dto)
        {
            dto.ProductReader = _productReader;

            var result = new PreShipmentCarbonDataDTO
            {
                OrderId = dto.OrderId,
                TimeStamp = dto.TimeStamp
            };

            SetStrategy(new ProductCF());
            result.ProductCF = _strategy.CalculateCarbon(dto);

            SetStrategy(new PackagingCF());
            result.PackagingCF = _strategy.CalculateCarbon(dto);

            SetStrategy(new StorageCF());
            result.StorageCF = _strategy.CalculateCarbon(dto);

            return result;
        }

        public void CreatePreShipmentCarbonData(PreShipmentDetailDTO dto)
        {
            var carbonData = CalculateAll(dto);
            _gateway.Insert(TableName, carbonData.OrderId, carbonData.TimeStamp, carbonData.ProductCF, carbonData.StorageCF, carbonData.PackagingCF);
        }

        public PreShipmentCarbonDataDTO? GetPreShipmentCarbonData(int orderId)
        {
            // Swapped DataTable for our custom RecordSet
            RecordSet recordSet = _gateway.FindByOrder(TableName, orderId);

            // Using the custom HasRows property we built earlier
            if (!recordSet.HasRows)
                return null;

            var row = recordSet.Rows[0];
            return new PreShipmentCarbonDataDTO
            {
                OrderId = Convert.ToInt32(row["order_id"]),
                TimeStamp = Convert.ToDateTime(row["time_stamp"]),
                ProductCF = Convert.ToSingle(row["product_cf"]),
                StorageCF = Convert.ToSingle(row["storage_cf"]),
                PackagingCF = Convert.ToSingle(row["packaging_cf"])
            };
        }

        public List<PreShipmentCarbonDataDTO> GetPreShipmentCarbonBreakdownByDate(DateOnly startDate, DateOnly endDate)
        {
            var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

            // Swapped DataTable for our custom RecordSet
            RecordSet recordSet = _gateway.FindByDateRange(TableName, startDateTime, endDateTime);
            var results = new List<PreShipmentCarbonDataDTO>();

            // Iterating through the RecordSet's list of dictionaries
            foreach (var row in recordSet.Rows)
            {
                results.Add(new PreShipmentCarbonDataDTO
                {
                    OrderId = Convert.ToInt32(row["order_id"]),
                    TimeStamp = Convert.ToDateTime(row["time_stamp"]),
                    ProductCF = Convert.ToSingle(row["product_cf"]),
                    StorageCF = Convert.ToSingle(row["storage_cf"]),
                    PackagingCF = Convert.ToSingle(row["packaging_cf"])
                });
            }

            return results;
        }
    }
}