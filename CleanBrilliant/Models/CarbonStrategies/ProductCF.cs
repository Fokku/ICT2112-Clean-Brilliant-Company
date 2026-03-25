using CleanBrilliant.DTO;
using CleanBrilliant.Models.Interfaces;

namespace CleanBrilliant.Models.CarbonStrategies
{
    public class ProductCF : ICarbonFootprintStrategy
    {
        private const float EmissionPerUnit = 0.5f; // Constant value representing carbon emissions per unit of product

        public float CalculateCarbon(PreShipmentDetailDTO dto)
        {
            if (dto == null || dto.ProductOrderDetailList == null)
                return 0f;

            float total = 0f;

            foreach (var item in dto.ProductOrderDetailList)
            {
                total += item.Quantity * EmissionPerUnit;
            }

            return total;
        }
    }
}