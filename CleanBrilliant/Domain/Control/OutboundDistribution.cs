using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.DomainInterface;
using EntityRouteData = CleanBrilliant.Domain.Entity.RouteData;

namespace CleanBrilliant.Domain.Control
{
    public class OutboundDistribution : RouteCalculationHandler, IEstimatedTimeService, ICustomerDistanceService
    {
        public const string CompanyLocationName = "SIT Campus Punggol";
        public const string CompanyPostalCode = "828608";

        private static readonly IReadOnlyDictionary<string, OutboundRouteProfile> MalaysiaRouteProfiles =
            new Dictionary<string, OutboundRouteProfile>(StringComparer.OrdinalIgnoreCase)
            {
                ["ship"] = new(
                    "ship",
                    "Jurong Port, Singapore",
                    1.31156,
                    103.71896,
                    "Jurong Port, Singapore",
                    "Port Klang, Malaysia",
                    2.9995164,
                    101.3914483,
                    326f,
                    2880f),
                ["air"] = new(
                    "air",
                    "Changi Airfreight Centre, Singapore",
                    1.3836,
                    103.9984,
                    "Changi Airfreight Centre, Singapore",
                    "Kuala Lumpur International Airport, Malaysia",
                    2.74558,
                    101.71,
                    299f,
                    71f),
                ["rail"] = new(
                    "rail",
                    "Woodlands Train Checkpoint, Singapore",
                    1.443778,
                    103.769611,
                    "JB Sentral, Malaysia",
                    "KL Sentral, Malaysia",
                    3.134747,
                    101.68617,
                    349f,
                    275f)
            };

        private readonly IOutboundDistributionGateway _gateway;
        public OutboundDistribution(
            IOSRMService osrmService,
            IPostalService postalService,
            ICarbonEntityFactory factory,
            IOutboundDistributionGateway gateway)
            : base(osrmService, postalService, factory)
        {
            _gateway = gateway;
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

        public async Task<OutboundRouteResult> CalculateOutboundRoute(
            string customerPostalCode,
            string shippingMethod,
            string countryCode)
        {
            var normalizedMethod = NormalizeShippingMethod(shippingMethod);

            if (IsSingaporeDestination(countryCode))
            {
                var localTruckLeg = await BuildTruckLeg(
                    CompanyLocationName,
                    CompanyPostalCode,
                    "Customer Location",
                    customerPostalCode);

                return BuildResult(normalizedMethod, [localTruckLeg]);
            }

            if (IsMalaysiaDestination(countryCode) &&
                MalaysiaRouteProfiles.TryGetValue(normalizedMethod, out var profile) &&
                !string.Equals(normalizedMethod, "truck", StringComparison.OrdinalIgnoreCase))
            {
                var truckToSingaporeHub = await BuildTruckLegToFixedPoint(
                    CompanyLocationName,
                    CompanyPostalCode,
                    profile.SingaporeHubName,
                    profile.SingaporeHubLatitude,
                    profile.SingaporeHubLongitude);
                var crossBorderLeg = BuildFixedLeg(profile);
                var truckToCustomer = await BuildTruckLegFromFixedPoint(
                    profile.MalaysiaHubName,
                    profile.MalaysiaHubLatitude,
                    profile.MalaysiaHubLongitude,
                    "Customer Location",
                    customerPostalCode);

                return BuildResult(normalizedMethod, [truckToSingaporeHub, crossBorderLeg, truckToCustomer]);
            }

            var directTruckLeg = await BuildTruckLeg(
                CompanyLocationName,
                CompanyPostalCode,
                "Customer Location",
                customerPostalCode);
            return BuildResult("truck", [directTruckLeg]);
        }

        protected override async Task<float> CalculateSpecificSegments()
        {
            if (_routeData == null) return 0f;
            return await Task.FromResult(_routeData.DistanceKm);
        }

        protected override async Task<float> EstimateTiming()
        {
            if (_routeData == null) return 0f;
            if (_routeData.DurationMin > 0f) return await Task.FromResult(_routeData.DurationMin);
            float minutes = _routeData.DistanceKm / 60f * 60f;
            return await Task.FromResult(minutes);
        }

        protected override async Task LogRoute(string referenceID)
        {
            if (_routeData == null) return;
            string routeDistId = Guid.NewGuid().ToString();
            await _gateway.Insert(referenceID, routeDistId, _routeData.DistanceKm, _routeData.DurationMin);
        }

        private async Task<OutboundLeg> BuildTruckLeg(
            string sourceName,
            string sourceQuery,
            string destinationName,
            string destinationQuery)
        {
            var sourceCoordinates = await GetCoordinates(sourceQuery);
            if (sourceCoordinates == null)
            {
                throw new InvalidOperationException($"Unable to resolve coordinates for '{sourceName}'.");
            }

            var destinationCoordinates = await GetCoordinates(destinationQuery);
            if (destinationCoordinates == null)
            {
                throw new InvalidOperationException($"Unable to resolve coordinates for '{destinationName}'.");
            }

            var metrics = await GetRouteMetrics(sourceCoordinates.Value, destinationCoordinates.Value);
            return new OutboundLeg(
                "truck",
                sourceName,
                destinationName,
                metrics.DistanceKm,
                metrics.DurationMin,
                sourceCoordinates.Value.Latitude,
                sourceCoordinates.Value.Longitude,
                destinationCoordinates.Value.Latitude,
                destinationCoordinates.Value.Longitude,
                "OSRM");
        }

        private async Task<OutboundLeg> BuildTruckLegToFixedPoint(
            string sourceName,
            string sourceQuery,
            string destinationName,
            double destinationLatitude,
            double destinationLongitude)
        {
            var sourceCoordinates = await GetCoordinates(sourceQuery);
            if (sourceCoordinates == null)
            {
                throw new InvalidOperationException($"Unable to resolve coordinates for '{sourceName}'.");
            }

            var metrics = await GetRouteMetrics(
                sourceCoordinates.Value,
                (destinationLatitude, destinationLongitude));

            return new OutboundLeg(
                "truck",
                sourceName,
                destinationName,
                metrics.DistanceKm,
                metrics.DurationMin,
                sourceCoordinates.Value.Latitude,
                sourceCoordinates.Value.Longitude,
                destinationLatitude,
                destinationLongitude,
                "OSRM");
        }

        private async Task<OutboundLeg> BuildTruckLegFromFixedPoint(
            string sourceName,
            double sourceLatitude,
            double sourceLongitude,
            string destinationName,
            string destinationQuery)
        {
            var destinationCoordinates = await GetCoordinates(destinationQuery);
            if (destinationCoordinates == null)
            {
                throw new InvalidOperationException($"Unable to resolve coordinates for '{destinationName}'.");
            }

            var metrics = await GetRouteMetrics(
                (sourceLatitude, sourceLongitude),
                destinationCoordinates.Value);

            return new OutboundLeg(
                "truck",
                sourceName,
                destinationName,
                metrics.DistanceKm,
                metrics.DurationMin,
                sourceLatitude,
                sourceLongitude,
                destinationCoordinates.Value.Latitude,
                destinationCoordinates.Value.Longitude,
                "OSRM");
        }

        private static OutboundLeg BuildFixedLeg(OutboundRouteProfile profile)
        {
            return new OutboundLeg(
                profile.Mode,
                profile.CrossBorderSourceName,
                profile.MalaysiaHubName,
                profile.DistanceKm,
                profile.DurationMin,
                profile.SingaporeHubLatitude,
                profile.SingaporeHubLongitude,
                profile.MalaysiaHubLatitude,
                profile.MalaysiaHubLongitude,
                "Hardcoded");
        }

        private static OutboundRouteResult BuildResult(
            string selectedMethod,
            IReadOnlyList<OutboundLeg> legs)
        {
            var totalDistance = legs.Sum(leg => leg.DistanceKm);
            var totalDuration = legs.Sum(leg => leg.DurationMin);

            return new OutboundRouteResult(
                selectedMethod,
                totalDistance,
                totalDuration,
                legs,
                $"{CompanyLocationName} -> {string.Join(" -> ", legs.Select(leg => leg.Destination))}");
        }

        private static string NormalizeShippingMethod(string shippingMethod)
        {
            var normalized = (shippingMethod ?? "truck").Trim().ToLowerInvariant();
            return normalized switch
            {
                "plane" => "air",
                "train" => "rail",
                _ => normalized
            };
        }

        private static bool IsSingaporeDestination(string countryCode)
        {
            var normalized = (countryCode ?? string.Empty).Trim().ToLowerInvariant();
            return normalized is "" or "sg" or "sgp" or "singapore";
        }

        private static bool IsMalaysiaDestination(string countryCode)
        {
            var normalized = (countryCode ?? string.Empty).Trim().ToLowerInvariant();
            return normalized is "my" or "mys" or "malaysia";
        }

        public sealed record OutboundRouteProfile(
            string Mode,
            string SingaporeHubName,
            double SingaporeHubLatitude,
            double SingaporeHubLongitude,
            string CrossBorderSourceName,
            string MalaysiaHubName,
            double MalaysiaHubLatitude,
            double MalaysiaHubLongitude,
            float DistanceKm,
            float DurationMin);

        public sealed record OutboundLeg(
            string Method,
            string Source,
            string Destination,
            float DistanceKm,
            float DurationMin,
            double? SourceLatitude,
            double? SourceLongitude,
            double? DestinationLatitude,
            double? DestinationLongitude,
            string SourceType);

        public sealed record OutboundRouteResult(
            string SelectedMethod,
            float TotalDistanceKm,
            float TotalDurationMin,
            IReadOnlyList<OutboundLeg> Legs,
            string Formula);
    }
}
