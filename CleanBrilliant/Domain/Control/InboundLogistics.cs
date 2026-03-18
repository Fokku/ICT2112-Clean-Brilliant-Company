using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.DomainInterface;

namespace CleanBrilliant.Domain.Control
{
    public class InboundLogistics : RouteCalculationHandler, IRestockDistanceService
    {
        private readonly IInboundLogisticsGateway _gateway;

        public string CompanyPostalCode { get; set; } = "";

        public InboundLogistics(
            IOSRMService osrmService,
            IPostalService postalService,
            ICarbonEntityFactory factory,
            IInboundLogisticsGateway gateway)
            : base(osrmService, postalService, factory)
        {
            _gateway = gateway;
        }

        public async Task<float> GetRestockDistance(string restockID)
        {
            var table = await _gateway.FindBy(restockID);
            if (table.Rows.Count == 0) return 0f;
            return Convert.ToSingle(table.Rows[0]["distance_km"]);
        }

        protected override async Task<float> CalculateSpecificSegments()
        {
            if (_routeData == null) return 0f;
            return await Task.FromResult(_routeData.DistanceKm);
        }

        protected override async Task<float> EstimateTiming()
        {
            if (_routeData == null) return 0f;
            float minutes = _routeData.DistanceKm / 60f * 60f;
            return await Task.FromResult(minutes);
        }

        protected override async Task LogRoute(string referenceID)
        {
            if (_routeData == null) return;
            float timeStamp = (float)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await _gateway.Insert(referenceID, _routeData.DistanceKm, _routeData.DurationMin, timeStamp);
        }
    }
}
