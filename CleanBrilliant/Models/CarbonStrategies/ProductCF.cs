using CleanBrilliant.DTO;
using CleanBrilliant.Models.Interfaces;

namespace CleanBrilliant.Models.CarbonStrategies
{
    public class ProductCF : ICarbonFootprintStrategy
    {
        
        public float CalculateCarbon(PreShipmentDetailDTO dto)
        {
            if (dto?.ProductOrderDetailList == null) return 0f;

            float total = 0f;
            foreach (var item in dto.ProductOrderDetailList)
            {
                // Use the reader inside the DTO
                var detail = dto.ProductReader?.GetProductDetail(item.ProductID);

                if (detail != null)
                {
                    total += detail.Carbon * item.Quantity;
                }
              
            }
            return total;
        }
    }
}