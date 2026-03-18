namespace CleanBrilliant.DTO
{
    public class PreShipmentCarbonDataDTO
    {
        public int OrderId { get; init; }
        public float ManufacturingCarbon { get; init; }
        public float PackagingCarbon { get; init; }
        public float TotalPreShipmentCarbon { get; init; }
    }
}
