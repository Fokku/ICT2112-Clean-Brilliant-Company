using CleanBrilliant.Data.Interfaces;
using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.DTO;
using System.Data;

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

        public async Task<CarbonLogPageDTO> GetCarbonLogs(int page, int pageSize)
        {
            var safePage = Math.Max(1, page);
            var safePageSize = Math.Clamp(pageSize, 1, 50);
            var rawEntries = new List<(string ReferenceId, string RecordType, float TotalCarbon, float? Timestamp)>();

            var customerTable = await _customerCarbonDataGateway.FindAll();
            foreach (DataRow row in customerTable.Rows)
            {
                rawEntries.Add(BuildRawLogEntry(row, "customer"));
            }

            var supplierTable = await _supplierCarbonDataGateway.FindAll();
            foreach (DataRow row in supplierTable.Rows)
            {
                rawEntries.Add(BuildRawLogEntry(row, "supplier"));
            }

            var orderedEntries = rawEntries
                .OrderByDescending(log => log.Timestamp ?? float.MinValue)
                .ThenBy(log => log.RecordType)
                .ThenBy(log => log.ReferenceId)
                .ToList();

            var totalCount = orderedEntries.Count;
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)safePageSize);
            var pageItems = orderedEntries
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .ToList();

            var items = new List<CarbonLogEntryDTO>();
            foreach (var rawEntry in pageItems)
            {
                items.Add(await BuildLogEntryAsync(rawEntry.ReferenceId, rawEntry.RecordType, rawEntry.TotalCarbon, rawEntry.Timestamp));
            }

            return new CarbonLogPageDTO
            {
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }

        private static (string ReferenceId, string RecordType, float TotalCarbon, float? Timestamp) BuildRawLogEntry(DataRow row, string recordType)
        {
            var isCustomer = string.Equals(recordType, "customer", StringComparison.OrdinalIgnoreCase);
            var referenceColumn = isCustomer ? "order_id" : "restock_id";
            var referenceId = Convert.ToString(row[referenceColumn]) ?? string.Empty;
            var totalCarbon = Convert.ToSingle(row["carbon_amount"]);
            float? timestamp = row["timestamp"] == DBNull.Value ? null : Convert.ToSingle(row["timestamp"]);

            return (referenceId, recordType, totalCarbon, timestamp);
        }

        private async Task<CarbonLogEntryDTO> BuildLogEntryAsync(string referenceId, string recordType, float totalCarbon, float? timestamp)
        {
            var isCustomer = string.Equals(recordType, "customer", StringComparison.OrdinalIgnoreCase);
            var preShipmentCarbon = GetPreShipmentCarbon(referenceId);
            var shippingCarbon = isCustomer
                ? await _getCarbonData.GetOrderShippingCarbon(referenceId)
                : await _getCarbonData.GetRestockShippingCarbon(referenceId);

            return new CarbonLogEntryDTO
            {
                ReferenceId = referenceId,
                RecordType = recordType,
                TotalCarbon = totalCarbon,
                ShippingCarbon = shippingCarbon,
                PreShipmentCarbon = preShipmentCarbon,
                Timestamp = timestamp,
                Formula = $"{preShipmentCarbon:0.##} + {shippingCarbon:0.##} = {totalCarbon:0.##} tonnes CO2"
            };
        }
    }
}
