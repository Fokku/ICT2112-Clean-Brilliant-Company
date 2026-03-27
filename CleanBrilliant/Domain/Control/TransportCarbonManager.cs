using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.Services;
using System.Data;
using EntityRouteData = CleanBrilliant.Domain.Entity.RouteData;

namespace CleanBrilliant.Domain.Control
{
    public class TransportCarbonManager : IDistanceCarbonService, IGetCarbonData, ISaveShippingMethod
    {
        private readonly ICustomerDistanceService _customerDistanceService;
        private readonly IRestockDistanceService _restockDistanceService;
        private readonly CleanBrilliant.Services.ICoefficientManager _coefficientManager;
        private readonly ICarbonEntityFactory _factory;
        private readonly ISupplierTransportCarbonGateway _supplierTransportCarbonGateway;
        private readonly ICustomerTransportCarbonGateway _customerTransportCarbonGateway;
        private readonly IOutboundDistributionGateway _outboundDistributionGateway;
        private readonly IInboundLogisticsGateway _inboundLogisticsGateway;
        private readonly IShippingMethodGateway _shippingMethodGateway;

        public TransportCarbonManager(
            ICustomerDistanceService customerDistanceService,
            IRestockDistanceService restockDistanceService,
            CleanBrilliant.Services.ICoefficientManager coefficientManager,
            ICarbonEntityFactory factory,
            ISupplierTransportCarbonGateway supplierTransportCarbonGateway,
            ICustomerTransportCarbonGateway customerTransportCarbonGateway,
            IOutboundDistributionGateway outboundDistributionGateway,
            IInboundLogisticsGateway inboundLogisticsGateway,
            IShippingMethodGateway shippingMethodGateway)
        {
            _customerDistanceService = customerDistanceService;
            _restockDistanceService = restockDistanceService;
            _coefficientManager = coefficientManager;
            _factory = factory;
            _supplierTransportCarbonGateway = supplierTransportCarbonGateway;
            _customerTransportCarbonGateway = customerTransportCarbonGateway;
            _outboundDistributionGateway = outboundDistributionGateway;
            _inboundLogisticsGateway = inboundLogisticsGateway;
            _shippingMethodGateway = shippingMethodGateway;
        }

        public async Task<float> CalculateCustomerDistanceCarbon(string orderID, string shippingMethod)
        {
            float customerDistanceKm = await _customerDistanceService.GetCustomerDistance(orderID);
            return CalculateCarbonFromDistance(customerDistanceKm, shippingMethod);
        }

        public Task<float> CalculateCustomerDistanceCarbon(float distanceKm, string shippingMethod)
        {
            return Task.FromResult(CalculateCarbonFromDistance(distanceKm, shippingMethod));
        }

        public async Task<float> CalculateSupplierDistanceCarbon(string restockID, string shippingMethod)
        {
            float supplierDistanceKm = await _restockDistanceService.GetRestockDistance(restockID);
            return CalculateCarbonFromDistance(supplierDistanceKm, "truck");
        }

        public Task<float> CalculateSupplierDistanceCarbon(float distanceKm, string shippingMethod)
        {
            return Task.FromResult(CalculateCarbonFromDistance(distanceKm, "truck"));
        }

        public async Task LogCustomerEmission(string orderID, float distanceCarbon)
        {
            float timeStamp = (float)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _customerTransportCarbonGateway.Insert(orderID, distanceCarbon, timeStamp);
        }

        public async Task LogRestockEmission(string restockID, float distanceCarbon)
        {
            float timeStamp = (float)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _supplierTransportCarbonGateway.Insert(restockID, distanceCarbon, timeStamp);
        }

        public async Task<float> GetDistanceCarbon(string orderID, EntityRouteData routeData)
        {
            string? shippingMethod = await _shippingMethodGateway.FindShippingMethod(orderID);
            if (string.IsNullOrEmpty(shippingMethod))
                shippingMethod = "truck";

            return CalculateCarbonFromDistance(routeData.DistanceKm, shippingMethod);
        }

        private float CalculateCarbonFromDistance(float distanceKm, string shippingMethod)
        {
            var normalizedMethod = NormalizeShippingMethod(shippingMethod);
            float configuredCoefficient = _coefficientManager.getEmission(normalizedMethod);
            return distanceKm * configuredCoefficient;
        }

        private static string NormalizeShippingMethod(string shippingMethod)
        {
            var normalized = (shippingMethod ?? "truck").Trim().ToLowerInvariant();
            return normalized switch
            {
                "air" => "plane",
                "rail" => "train",
                _ => normalized
            };
        }

        public async Task<float> GetOrderShippingCarbon(string orderID)
        {
            var table = await _customerTransportCarbonGateway.FindBy(orderID);
            if (table.Rows.Count == 0) return 0f;
            return Convert.ToSingle(table.Rows[0]["carbon_amount"]);
        }

        public async Task<(float ShippingCarbon, float? Timestamp)> GetOrderShippingCarbonRecord(string orderID)
        {
            var table = await _customerTransportCarbonGateway.FindBy(orderID);
            if (table.Rows.Count == 0) return (0f, null);

            var row = table.Rows[0];
            var shippingCarbon = Convert.ToSingle(row["carbon_amount"]);
            float? timestamp = row["timestamp"] == DBNull.Value ? null : Convert.ToSingle(row["timestamp"]);
            return (shippingCarbon, timestamp);
        }

        public async Task<object?> GetOrderShippingBreakdown(string orderID)
        {
            var transportTable = await _customerTransportCarbonGateway.FindBy(orderID);
            if (transportTable.Rows.Count == 0) return null;

            var distanceTable = await _outboundDistributionGateway.FindBy(orderID);
            return BuildShippingBreakdown(orderID, "order", transportTable.Rows[0], distanceTable.Rows.Count > 0 ? distanceTable.Rows[0] : null);
        }

        public async Task<object?> GetOutboundSummary(string orderID)
        {
            var table = await _outboundDistributionGateway.FindBy(orderID);
            if (table.Rows.Count == 0) return null;

            var row = table.Rows[0];
            return new
            {
                orderID = Convert.ToString(row["order_id"]) ?? orderID,
                customerRouteDistID = row.Table.Columns.Contains("customer_route_dist_id") && row["customer_route_dist_id"] != DBNull.Value
                    ? Convert.ToString(row["customer_route_dist_id"])
                    : null,
                distanceKm = row["distance_km"] == DBNull.Value ? (float?)null : Convert.ToSingle(row["distance_km"]),
                durationMin = row["duration_min"] == DBNull.Value ? (float?)null : Convert.ToSingle(row["duration_min"]),
                timestamp = row.Table.Columns.Contains("timestamp") && row["timestamp"] != DBNull.Value
                    ? Convert.ToSingle(row["timestamp"])
                    : (float?)null
            };
        }

        public async Task<float> GetRestockShippingCarbon(string restockID)
        {
            var table = await _supplierTransportCarbonGateway.FindBy(restockID);
            if (table.Rows.Count == 0) return 0f;
            return Convert.ToSingle(table.Rows[0]["carbon_amount"]);
        }

        public async Task<(float ShippingCarbon, float? Timestamp)> GetRestockShippingCarbonRecord(string restockID)
        {
            var table = await _supplierTransportCarbonGateway.FindBy(restockID);
            if (table.Rows.Count == 0) return (0f, null);

            var row = table.Rows[0];
            var shippingCarbon = Convert.ToSingle(row["carbon_amount"]);
            float? timestamp = row["timestamp"] == DBNull.Value ? null : Convert.ToSingle(row["timestamp"]);
            return (shippingCarbon, timestamp);
        }

        public async Task<object?> GetRestockShippingBreakdown(string restockID)
        {
            var transportTable = await _supplierTransportCarbonGateway.FindBy(restockID);
            if (transportTable.Rows.Count == 0) return null;

            var distanceTable = await _inboundLogisticsGateway.FindBy(restockID);
            return BuildShippingBreakdown(restockID, "restock", transportTable.Rows[0], distanceTable.Rows.Count > 0 ? distanceTable.Rows[0] : null);
        }

        public async Task<object?> GetInboundSummary(string restockID)
        {
            var table = await _inboundLogisticsGateway.FindBy(restockID);
            if (table.Rows.Count == 0) return null;

            var row = table.Rows[0];
            return new
            {
                restockID = Convert.ToString(row["restock_id"]) ?? restockID,
                supplierRouteDistID = row.Table.Columns.Contains("supplier_route_dist_id") && row["supplier_route_dist_id"] != DBNull.Value
                    ? Convert.ToString(row["supplier_route_dist_id"])
                    : null,
                distanceKm = row["distance_km"] == DBNull.Value ? (float?)null : Convert.ToSingle(row["distance_km"]),
                durationMin = row["duration_min"] == DBNull.Value ? (float?)null : Convert.ToSingle(row["duration_min"]),
                timestamp = row.Table.Columns.Contains("timestamp") && row["timestamp"] != DBNull.Value
                    ? Convert.ToSingle(row["timestamp"])
                    : (float?)null
            };
        }

        private object BuildShippingBreakdown(string referenceID, string type, DataRow transportRow, DataRow? distanceRow)
        {
            var method = transportRow.Table.Columns.Contains("shipping_method") && transportRow["shipping_method"] != DBNull.Value
                ? Convert.ToString(transportRow["shipping_method"]) ?? "Unknown"
                : "Unknown";

            if (string.Equals(type, "restock", StringComparison.OrdinalIgnoreCase))
            {
                method = "truck";
            }

            float shippingCarbon = Convert.ToSingle(transportRow["carbon_amount"]);
            float? timestamp = transportRow["timestamp"] == DBNull.Value ? null : Convert.ToSingle(transportRow["timestamp"]);
            float? distanceKm = distanceRow == null || distanceRow["distance_km"] == DBNull.Value
                ? null
                : Convert.ToSingle(distanceRow["distance_km"]);
            float? coefficient = method == "Unknown" ? null : _coefficientManager.getEmission(NormalizeShippingMethod(method));
            var formula = distanceKm.HasValue && coefficient.HasValue
                ? $"{distanceKm.Value:0.##} km × {coefficient.Value:0.####} = {shippingCarbon:0.##} tonnes CO2"
                : "Distance or shipping method data not available for a full breakdown.";

            return new
            {
                referenceID,
                type,
                shippingMethod = method,
                distanceKm,
                coefficient,
                shippingCarbon,
                timestamp,
                formula
            };
        }

        public async Task SaveShippingMethod(string orderID, string shippingMethod)
        {
            if (string.IsNullOrWhiteSpace(orderID) || string.IsNullOrWhiteSpace(shippingMethod))
            {
                return;
            }

            var existingMethod = await _shippingMethodGateway.FindShippingMethod(orderID);
            if (string.IsNullOrWhiteSpace(existingMethod))
            {
                await _shippingMethodGateway.InsertShippingMethod(orderID, shippingMethod);
                return;
            }

            if (!string.Equals(existingMethod, shippingMethod, StringComparison.OrdinalIgnoreCase))
            {
                await _shippingMethodGateway.UpdateShippingMethod(orderID, shippingMethod);
            }
        }
    }
}
