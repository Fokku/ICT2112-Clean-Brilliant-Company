using CleanBrilliant.DTO;

namespace CleanBrilliant.Data.Interfaces
{
    public interface IPreShipmentCarbonWriter
    {
        void CreatePreShipmentCarbonData(PreShipmentDetailDTO preShipmentDetail);
    }
}