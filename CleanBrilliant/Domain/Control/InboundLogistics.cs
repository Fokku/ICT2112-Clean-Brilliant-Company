using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.DTO;

namespace CleanBrilliant.Domain.Control
{
    public class InboundLogistics : RouteCalculationHandler, IRestockDistanceService
    {
        public const string CompanyLocationName = "SIT Campus Punggol";
        public const string CompanyPostalCode = "828608";

        private static readonly IReadOnlyList<InboundWarehouse> SupportedWarehouses =
        [
            new("sit-campus-w-block-punggol", "SIT Campus W Block, Punggol", CompanyPostalCode),
            new("sit-dover", "SIT Dover", "138683"),
            new("sit-nyp", "SIT NYP", "567739")
        ];

        private readonly IInboundLogisticsGateway _gateway;

        public InboundLogistics(
            IOSRMService osrmService,
            IPostalService postalService,
            ICarbonEntityFactory factory,
            IInboundLogisticsGateway gateway)
            : base(osrmService, postalService, factory)
        {
            _gateway = gateway;
        }

        public IReadOnlyList<InboundWarehouse> GetSupportedWarehouses() => SupportedWarehouses;

        public InboundWarehouse? GetSupportedWarehouse(string warehouseId)
        {
            return SupportedWarehouses.FirstOrDefault(warehouse =>
                string.Equals(warehouse.Id, warehouseId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<PostalRouteCalculationDTO> CalculateInboundRoute(string warehouseId)
        {
            var warehouse = GetSupportedWarehouse(warehouseId);
            if (warehouse == null)
            {
                throw new InvalidOperationException(
                    "Inbound logistics only supports SIT Campus W Block, SIT Dover, and SIT NYP.");
            }

            return await CalculatePostalRoute(warehouse.PostalCode, CompanyPostalCode);
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
            string routeDistId = Guid.NewGuid().ToString();
            await _gateway.Insert(referenceID, routeDistId, _routeData.DistanceKm, _routeData.DurationMin, timeStamp);
        }

        public sealed record InboundWarehouse(string Id, string Name, string PostalCode);
    }
}
