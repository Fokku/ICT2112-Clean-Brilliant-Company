using CleanBrilliant.DTO;
namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IPreShipmentCarbonReader
    {
        Task<PreShipmentCarbonDataDTO> GetPreShipmentCarbonData(int orderID);
    }
}
