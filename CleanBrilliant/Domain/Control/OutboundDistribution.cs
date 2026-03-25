using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.DomainInterface;
using EntityRouteData = CleanBrilliant.Domain.Entity.RouteData;

namespace CleanBrilliant.Domain.Control
{
    public class OutboundDistribution : RouteCalculationHandler, IEstimatedTimeService, ICustomerDistanceService
    {
        private readonly IOutboundDistributionGateway _gateway;
        private readonly TransportHubManager _hubManager;

        public OutboundDistribution(
            IOSRMService osrmService,
            IPostalService postalService,
            ICarbonEntityFactory factory,
            IOutboundDistributionGateway gateway,
            TransportHubManager hubManager)
            : base(osrmService, postalService, factory)
        {
            _gateway = gateway;
            _hubManager = hubManager;
        }

        public async Task<float> GetCustomerDistance(string orderID)
        {
            var table = await _gateway.FindBy(orderID);
            if (table.Rows.Count == 0) return 0f;
            return Convert.ToSingle(table.Rows[0]["distance_km"]);
        }

        public async Task<float> GetOrderEstimatedTime(string orderID, EntityRouteData routeData)
        {
            return await Task.FromResult(routeData.DurationMin);
        }

        public double GetComptoHubDistance()
        {
            return _hubManager.GetTransportHubDistance("truck");
        }

        public double GetHubtoCustDistance()
        {
            return _hubManager.GetTransportHubDistance("truck");
        }

        protected override async Task<float> CalculateSpecificSegments()
        {
            if (_routeData == null) return 0f;
            double compToHub = GetComptoHubDistance();
            double hubToCust = GetHubtoCustDistance();
            _routeData.DistanceKm += (float)(compToHub + hubToCust);
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
            string routeDistId = Guid.NewGuid().ToString();
            await _gateway.Insert(referenceID, routeDistId, _routeData.DistanceKm, _routeData.DurationMin);
        }
    }
}
