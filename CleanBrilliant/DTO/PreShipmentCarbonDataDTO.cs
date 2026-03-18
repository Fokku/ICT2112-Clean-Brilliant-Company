namespace CleanBrilliant.DTO
{
    public class PreShipmentCarbonDataDTO
    {
        public int OrderId { get; set; }
        public float ManufacturingCarbon { get; set; }
        public float PackagingCarbon { get; set; }
        public float TotalPreShipmentCarbon { get; set; }
    }
}
