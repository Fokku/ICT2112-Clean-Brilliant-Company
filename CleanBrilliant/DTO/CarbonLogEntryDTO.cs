namespace CleanBrilliant.DTO
{
    public class CarbonLogEntryDTO
    {
        public string ReferenceId { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public float TotalCarbon { get; set; }
        public float? ShippingCarbon { get; set; }
        public float? PreShipmentCarbon { get; set; }
        public float? Timestamp { get; set; }
        public string Formula { get; set; } = string.Empty;
    }
}
