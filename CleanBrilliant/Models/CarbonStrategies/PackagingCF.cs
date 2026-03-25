using CleanBrilliant.DTO;
using CleanBrilliant.Models.Interfaces;

namespace CleanBrilliant.Models.CarbonStrategies
{
    public class PackagingCF : ICarbonFootprintStrategy
    {
        private const float EmissionPerPackage = 0.2f; // Constant value representing carbon emissions per package

        public float CalculateCarbon(PreShipmentDetailDTO dto)
        {
            if (dto == null)
                return 0f;

            return dto.PackageQuantity * EmissionPerPackage;
        }
    }
}