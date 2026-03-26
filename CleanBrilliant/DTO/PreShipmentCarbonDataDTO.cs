using System;
namespace CleanBrilliant.DTO
{
    public class PreShipmentCarbonDataDTO
    {
       public int OrderId { get; set; }
        public DateTime TimeStamp { get; set; }
        public float ProductCF { get; set; }
        public float StorageCF { get; set; }
        public float PackagingCF { get; set; }
        
        // NEW: Shipment CF
        public float ShipmentCF { get; set; }

        // NEW: Update Total to include ShipmentCF
        public float TotalCF => ProductCF + StorageCF + PackagingCF + ShipmentCF;

        // Compatibility aliases for the module-3 naming model.
        public float ManufacturingCarbon
        {
            get => ProductCF;
            set => ProductCF = value;
        }

        public float PackagingCarbon
        {
            get => PackagingCF;
            set => PackagingCF = value;
        }

        private float? _totalPreShipmentCarbon;
        public float TotalPreShipmentCarbon
        {
            get => _totalPreShipmentCarbon ?? TotalCF;
            set => _totalPreShipmentCarbon = value;
        }
    }
}
