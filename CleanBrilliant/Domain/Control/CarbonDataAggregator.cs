using CleanBrilliant.Data.Interfaces;
using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.DomainInterface;

namespace CleanBrilliant.Domain.Control
{
    public class CarbonDataAggregator : ITotalCarbonService
    {
        private readonly IPreShipmentCarbonReader _preShipmentCarbonReader;
        private readonly IGetCarbonData _getCarbonData;
        private readonly ICarbonEntityFactory _factory;
        private readonly ISupplierCarbonDataGateway _supplierCarbonDataGateway;
        private readonly ICustomerCarbonDataGateway _customerCarbonDataGateway;

        public CarbonDataAggregator(
            IPreShipmentCarbonReader preShipmentCarbonReader,
            IGetCarbonData getCarbonData,
            ICarbonEntityFactory factory,
            ISupplierCarbonDataGateway supplierCarbonDataGateway,
            ICustomerCarbonDataGateway customerCarbonDataGateway)
        {
            _preShipmentCarbonReader = preShipmentCarbonReader;
            _getCarbonData = getCarbonData;
            _factory = factory;
            _supplierCarbonDataGateway = supplierCarbonDataGateway;
            _customerCarbonDataGateway = customerCarbonDataGateway;
        }

        public async Task<float> GetOrderCarbon(string orderID, string countryCode, string postalCode, ICarbonEntityFactory factory)
        {
            float preShipmentCarbon = GetPreShipmentCarbon(orderID);

            float shippingCarbon = await _getCarbonData.GetOrderShippingCarbon(orderID);
            float totalCarbon = preShipmentCarbon + shippingCarbon;

            float timeStamp = (float)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _customerCarbonDataGateway.Insert(orderID, totalCarbon, timeStamp);

            return totalCarbon;
        }

        public async Task<float> GetSupplierOrderCarbon(string restockID, string postalCode, string countryCode, ICarbonEntityFactory factory)
        {
            float preShipmentCarbon = GetPreShipmentCarbon(restockID);
            float shippingCarbon = await _getCarbonData.GetRestockShippingCarbon(restockID);
            float totalCarbon = preShipmentCarbon + shippingCarbon;

            float timeStamp = (float)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _supplierCarbonDataGateway.Insert(restockID, totalCarbon, timeStamp);

            return totalCarbon;
        }

        public async Task<(float TotalCarbon, float? Timestamp)> GetOrderCarbonSummary(string orderID)
        {
            var table = await _customerCarbonDataGateway.FindBy(orderID);
            if (table.Rows.Count == 0) return (0f, null);

            var row = table.Rows[0];
            var totalCarbon = Convert.ToSingle(row["carbon_amount"]);
            float? timestamp = row["timestamp"] == DBNull.Value ? null : Convert.ToSingle(row["timestamp"]);
            return (totalCarbon, timestamp);
        }

        public async Task<object?> GetOrderCarbonSummaryBreakdown(string orderID)
        {
            var summary = await GetOrderCarbonSummary(orderID);
            if (summary.Timestamp == null)
            {
                return null;
            }

            float preOrderCarbon = GetPreShipmentCarbon(orderID);
            float shippingCarbon = await _getCarbonData.GetOrderShippingCarbon(orderID);
            float totalCarbon = preOrderCarbon + shippingCarbon;

            return new
            {
                orderID,
                preOrderCarbon,
                shippingCarbon,
                totalCarbon,
                timestamp = summary.Timestamp,
                formula = $"{preOrderCarbon:0.##} + {shippingCarbon:0.##} = {totalCarbon:0.##} tonnes CO2"
            };
        }

        public async Task<(float TotalCarbon, float? Timestamp)> GetRestockCarbonSummary(string restockID)
        {
            var table = await _supplierCarbonDataGateway.FindBy(restockID);
            if (table.Rows.Count == 0) return (0f, null);

            var row = table.Rows[0];
            var totalCarbon = Convert.ToSingle(row["carbon_amount"]);
            float? timestamp = row["timestamp"] == DBNull.Value ? null : Convert.ToSingle(row["timestamp"]);
            return (totalCarbon, timestamp);
        }

        public async Task<object?> GetRestockCarbonSummaryBreakdown(string restockID)
        {
            var summary = await GetRestockCarbonSummary(restockID);
            if (summary.Timestamp == null)
            {
                return null;
            }

            float preOrderCarbon = GetPreShipmentCarbon(restockID);
            float shippingCarbon = await _getCarbonData.GetRestockShippingCarbon(restockID);
            float totalCarbon = preOrderCarbon + shippingCarbon;

            return new
            {
                restockID,
                preOrderCarbon,
                shippingCarbon,
                totalCarbon,
                timestamp = summary.Timestamp,
                formula = $"{preOrderCarbon:0.##} + {shippingCarbon:0.##} = {totalCarbon:0.##} tonnes CO2"
            };
        }

        private float GetPreShipmentCarbon(string referenceId)
        {
            if (!int.TryParse(referenceId, out int parsedId))
            {
                return 0f;
            }

            var preShipmentData = _preShipmentCarbonReader.GetPreShipmentCarbonData(parsedId);
            return preShipmentData?.TotalPreShipmentCarbon ?? 0f;
        }

        public async Task<List<float>> GetCarbonLogs()
        {
            var results = new List<float>();

            var customerTable = await _customerCarbonDataGateway.FindAll();
            foreach (System.Data.DataRow row in customerTable.Rows)
            {
                results.Add(Convert.ToSingle(row["carbon_amount"]));
            }

            var supplierTable = await _supplierCarbonDataGateway.FindAll();
            foreach (System.Data.DataRow row in supplierTable.Rows)
            {
                results.Add(Convert.ToSingle(row["carbon_amount"]));
            }

            return results;
        }
    }
}
