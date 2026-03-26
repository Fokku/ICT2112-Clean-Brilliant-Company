namespace CleanBrilliant.DTO
{
    public class PostalRouteCalculationDTO
    {
        public string SourcePostalCode { get; set; } = string.Empty;
        public string DestinationPostalCode { get; set; } = string.Empty;
        public double SourceLatitude { get; set; }
        public double SourceLongitude { get; set; }
        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }
        public float DistanceKm { get; set; }
        public float DurationMin { get; set; }
        public string Formula { get; set; } = string.Empty;
    }
}
