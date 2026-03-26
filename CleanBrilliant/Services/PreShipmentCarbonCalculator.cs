using System.Data;
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
            if (_strategy is ProductCF)
            {
                float totalProductCarbon = 0f;
                foreach (var item in dto.ProductOrderDetailList)
                {
                    var detail = _productReader.GetProductDetail(item.ProductID);
                    if (detail != null)
                    {
                        totalProductCarbon += detail.Carbon * item.Quantity;
                    }
                }
                return totalProductCarbon;
            }

            return _strategy.CalculateCarbon(dto);
        }

        public PreShipmentCarbonDataDTO CalculateAll(PreShipmentDetailDTO dto)
        {
            var result = new PreShipmentCarbonDataDTO
            {
                OrderId = dto.OrderId,
                TimeStamp = dto.TimeStamp,
                ShipmentCF = dto.ShipmentCF ?? 0f // Map the optional value, default to 0
            };

            SetStrategy(new ProductCF());
            result.ProductCF = CalculateCarbon(dto);

            SetStrategy(new PackagingCF());
            result.PackagingCF = CalculateCarbon(dto);

            SetStrategy(new StorageCF());
            result.StorageCF = CalculateCarbon(dto);

            return result;
        }

        public void CreatePreShipmentCarbonData(PreShipmentDetailDTO dto)
        {
            var carbonData = CalculateAll(dto);
            // Pass the new ShipmentCF to the gateway
            _gateway.Insert(TableName, carbonData.OrderId, carbonData.TimeStamp, carbonData.ProductCF, carbonData.StorageCF, carbonData.PackagingCF, carbonData.ShipmentCF);
        }

        public PreShipmentCarbonDataDTO? GetPreShipmentCarbonData(int orderId)
        {
            DataTable recordSet = _gateway.FindByOrder(TableName, orderId);
            
            if (recordSet.Rows.Count == 0)
                return null;

            DataRow row = recordSet.Rows[0];
            return new PreShipmentCarbonDataDTO
            {
                OrderId = Convert.ToInt32(row["order_id"]),
                TimeStamp = Convert.ToDateTime(row["time_stamp"]),
                ProductCF = Convert.ToSingle(row["product_cf"]),
                StorageCF = Convert.ToSingle(row["storage_cf"]),
                PackagingCF = Convert.ToSingle(row["packaging_cf"]),
                ShipmentCF = row["shipment_cf"] != DBNull.Value ? Convert.ToSingle(row["shipment_cf"]) : 0f
            };
        }

        public List<PreShipmentCarbonDataDTO> GetPreShipmentCarbonBreakdownByDate(DateOnly startDate, DateOnly endDate)
        {
            var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

            DataTable recordSet = _gateway.FindByDateRange(TableName, startDateTime, endDateTime);
            var results = new List<PreShipmentCarbonDataDTO>();

            foreach (DataRow row in recordSet.Rows)
            {
                results.Add(new PreShipmentCarbonDataDTO
                {
                    OrderId = Convert.ToInt32(row["order_id"]),
                    TimeStamp = Convert.ToDateTime(row["time_stamp"]),
                    ProductCF = Convert.ToSingle(row["product_cf"]),
                    StorageCF = Convert.ToSingle(row["storage_cf"]),
                    PackagingCF = Convert.ToSingle(row["packaging_cf"]),
                    ShipmentCF = row["shipment_cf"] != DBNull.Value ? Convert.ToSingle(row["shipment_cf"]) : 0f
                });
            }

            return results;
        }
    }
}