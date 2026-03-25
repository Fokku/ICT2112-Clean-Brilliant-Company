using CleanBrilliant.DTO;
using CleanBrilliant.Models.Interfaces;

namespace CleanBrilliant.Models.CarbonStrategies
{
    public class StorageCF : ICarbonFootprintStrategy
    {
        private const float EmissionPerStorageDay = 0.1f;

        public float CalculateCarbon(PreShipmentDetailDTO dto)
        {
            if (dto == null || dto.ReceivedDateList == null)
                return 0f;

            float total = 0f;
            var today = dto.TimeStamp.Date;

            foreach (var receivedDate in dto.ReceivedDateList)
            {
                int daysStored = (today - receivedDate.Date).Days;
                if (daysStored > 0)
                {
                    total += daysStored * EmissionPerStorageDay;
                }
            }

            return total;
        }
    }
}