using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.Services;
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
        private readonly IShippingMethodGateway _shippingMethodGateway;
        private readonly CarbonAnalysis _carbonAnalysis;

        public TransportCarbonManager(
            ICustomerDistanceService customerDistanceService,
            IRestockDistanceService restockDistanceService,
            CleanBrilliant.Services.ICoefficientManager coefficientManager,
            ICarbonEntityFactory factory,
            ISupplierTransportCarbonGateway supplierTransportCarbonGateway,
            ICustomerTransportCarbonGateway customerTransportCarbonGateway,
            IShippingMethodGateway shippingMethodGateway,
            CarbonAnalysis carbonAnalysis)
        {
            _customerDistanceService = customerDistanceService;
            _restockDistanceService = restockDistanceService;
            _coefficientManager = coefficientManager;
            _factory = factory;
            _supplierTransportCarbonGateway = supplierTransportCarbonGateway;
            _customerTransportCarbonGateway = customerTransportCarbonGateway;
            _shippingMethodGateway = shippingMethodGateway;
            _carbonAnalysis = carbonAnalysis;
        }

        public async Task<float> CalculateCustomerDistanceCarbon(string orderID, string shippingMethod)
        {
            float distance = await _customerDistanceService.GetCustomerDistance(orderID);
            float baseCarbon = _carbonAnalysis.EstimateShippingCarbon(shippingMethod, distance);
            return ApplyConfiguredCoefficient(shippingMethod, baseCarbon);
        }

        public async Task<float> CalculateSupplierDistanceCarbon(string restockID, string shippingMethod)
        {
            float distance = await _restockDistanceService.GetRestockDistance(restockID);
            float baseCarbon = _carbonAnalysis.EstimateShippingCarbon(shippingMethod, distance);
            return ApplyConfiguredCoefficient(shippingMethod, baseCarbon);
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

            float baseCarbon = _carbonAnalysis.EstimateShippingCarbon(shippingMethod, routeData.DistanceKm);
            return ApplyConfiguredCoefficient(shippingMethod, baseCarbon);
        }

        private float ApplyConfiguredCoefficient(string shippingMethod, float baseCarbon)
        {
            var normalizedMethod = NormalizeShippingMethod(shippingMethod);
            float configuredCoefficient = _coefficientManager.getEmission(normalizedMethod);
            return baseCarbon * configuredCoefficient;
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

        public async Task<float> GetRestockShippingCarbon(string restockID)
        {
            var table = await _supplierTransportCarbonGateway.FindBy(restockID);
            if (table.Rows.Count == 0) return 0f;
            return Convert.ToSingle(table.Rows[0]["carbon_amount"]);
        }

        public async Task SaveShippingMethod(string shippingMethod)
        {
            // Placeholder - needs order context
            await Task.CompletedTask;
        }
    }
}
