using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.DTO;

namespace CleanBrilliant.Stubs
{
    public class PreShipmentCarbonReaderStub : IPreShipmentCarbonReader
    {
        public async Task<PreShipmentCarbonDataDTO> GetPreShipmentCarbonData(int orderID)
        {
            await Task.CompletedTask;
            return new PreShipmentCarbonDataDTO
            {
                OrderId = orderID,
                ManufacturingCarbon = 5.0f,
                PackagingCarbon = 2.0f,
                TotalPreShipmentCarbon = 7.0f
            };
        }
    }
}
