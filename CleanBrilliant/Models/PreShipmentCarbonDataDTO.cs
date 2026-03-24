namespace Module3.Models
{
    public class PreShipmentCarbonDataDTO
    {
        public int OrderId { get; set; }
        public DateTime TimeStamp { get; set; }
        public float ProductCF { get; set; }
        public float StorageCF { get; set; }
        public float PackagingCF { get; set; }
    }
}