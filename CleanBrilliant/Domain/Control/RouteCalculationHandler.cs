using CleanBrilliant.Domain.DomainInterface;
using EntityRouteData = CleanBrilliant.Domain.Entity.RouteData;

namespace CleanBrilliant.Domain.Control
{
    public abstract class RouteCalculationHandler
    {
        protected readonly IOSRMService _osrmService;
        private readonly IPostalService _postalService;
        private readonly ICarbonEntityFactory _factory;
        protected EntityRouteData? _routeData;

        protected RouteCalculationHandler(IOSRMService osrmService, IPostalService postalService, ICarbonEntityFactory factory)
        {
            _osrmService = osrmService;
            _postalService = postalService;
            _factory = factory;
        }

        public async Task<EntityRouteData> CalculateRoute(string refID, string srcPostal, string destPostal)
        {
            _routeData = _factory.CreateRouteData(refID);
            var (source, dest) = await ObtainLatLng(srcPostal, destPostal);
            float distance = await GetRouteDistance(source.Longitude, source.Latitude, dest.Longitude, dest.Latitude);
            _routeData.DistanceKm = distance;
            await CalculateSpecificSegments();
            _routeData.DurationMin = await EstimateTiming();
            await LogRoute(refID);
            return _routeData;
        }

        public async Task<((double Longitude, double Latitude) Source, (double Longitude, double Latitude) Dest)> ObtainLatLng(string sourcePostal, string destPostal)
        {
            return await _postalService.GetPostalConversion(sourcePostal, destPostal);
        }

        public async Task<float> GetRouteDistance(double srcLng, double srcLat, double destLng, double destLat)
        {
            return await _osrmService.GetRouteDistance(srcLng, srcLat, destLng, destLat);
        }

        protected abstract Task LogRoute(string referenceID);
        protected abstract Task<float> CalculateSpecificSegments();
        protected abstract Task<float> EstimateTiming();
    }
}
