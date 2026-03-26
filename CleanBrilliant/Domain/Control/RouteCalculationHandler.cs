using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.DTO;
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
            var route = await CalculatePostalRoute(srcPostal, destPostal);
            _routeData.DistanceKm = route.DistanceKm;
            _routeData.DurationMin = route.DurationMin;
            await CalculateSpecificSegments();
            _routeData.DurationMin = await EstimateTiming();
            await LogRoute(refID);
            return _routeData;
        }

        public async Task<PostalRouteCalculationDTO> CalculatePostalRoute(string sourcePostal, string destPostal)
        {
            var sourceCoordinates = await GetCoordinates(sourcePostal);
            var destinationCoordinates = await GetCoordinates(destPostal);

            if (sourceCoordinates == null)
            {
                throw new InvalidOperationException($"Unable to resolve coordinates for postal code '{sourcePostal}'.");
            }

            if (destinationCoordinates == null)
            {
                throw new InvalidOperationException($"Unable to resolve coordinates for postal code '{destPostal}'.");
            }

            var route = await GetRouteMetrics(sourceCoordinates.Value, destinationCoordinates.Value);

            return new PostalRouteCalculationDTO
            {
                SourcePostalCode = sourcePostal,
                DestinationPostalCode = destPostal,
                SourceLatitude = sourceCoordinates.Value.Latitude,
                SourceLongitude = sourceCoordinates.Value.Longitude,
                DestinationLatitude = destinationCoordinates.Value.Latitude,
                DestinationLongitude = destinationCoordinates.Value.Longitude,
                DistanceKm = route.DistanceKm,
                DurationMin = route.DurationMin,
                Formula = $"({sourceCoordinates.Value.Latitude:0.######}, {sourceCoordinates.Value.Longitude:0.######}) -> ({destinationCoordinates.Value.Latitude:0.######}, {destinationCoordinates.Value.Longitude:0.######})"
            };
        }

        protected async Task<(double Latitude, double Longitude)?> GetCoordinates(string postalCode)
        {
            return await _postalService.GetCoordinates(postalCode);
        }

        protected async Task<((double Latitude, double Longitude) Source, (double Latitude, double Longitude) Destination)?> ObtainLatLng(string sourcePostal, string destPostal)
        {
            var sourceCoordinates = await GetCoordinates(sourcePostal);
            var destinationCoordinates = await GetCoordinates(destPostal);

            if (sourceCoordinates == null || destinationCoordinates == null)
            {
                return null;
            }

            return (sourceCoordinates.Value, destinationCoordinates.Value);
        }

        protected async Task<(float DistanceKm, float DurationMin)> GetRouteMetrics(
            (double Latitude, double Longitude) source,
            (double Latitude, double Longitude) destination)
        {
            return await _osrmService.GetRoute(
                source.Longitude,
                source.Latitude,
                destination.Longitude,
                destination.Latitude);
        }

        protected abstract Task LogRoute(string referenceID);
        protected abstract Task<float> CalculateSpecificSegments();
        protected abstract Task<float> EstimateTiming();
    }
}
