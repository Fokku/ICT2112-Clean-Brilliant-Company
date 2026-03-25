namespace CleanBrilliant.Domain.Entity
{
    public abstract class RouteData
    {
        public float DistanceKm { get; set; }
        public float DurationMin { get; set; }
        public float Timestamp { get; set; }

        public abstract string GetReferenceId();
        public abstract void SetReferenceId(string id);

        public float CalculateRouteDistance(double longitude, double latitude)
        {
            return DistanceKm; // Placeholder — actual calc via OSRM
        }
    }

    public class CustomerRouteData : RouteData
    {
        public string OrderId { get; set; } = "";
        public override string GetReferenceId() => OrderId;
        public override void SetReferenceId(string id) => OrderId = id;
    }

    public class SupplierRouteData : RouteData
    {
        public string RestockId { get; set; } = "";
        public override string GetReferenceId() => RestockId;
        public override void SetReferenceId(string id) => RestockId = id;
    }
}
