using CleanBrilliant.DTO;

namespace CleanBrilliant.Models.Interfaces
{
    public interface ICarbonFootprintStrategy
    {
        float CalculateCarbon(PreShipmentDetailDTO dto);
    }
}