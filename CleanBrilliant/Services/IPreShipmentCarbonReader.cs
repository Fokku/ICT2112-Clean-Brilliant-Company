using System.Globalization;
using CleanBrilliant.Models;

namespace CleanBrilliant.Services
{
    public interface IPreShipmentCarbonReader
    {
        PreShipmentCarbonDataDTO GetPreShipmentCarbonData(int orderId)
        {
            var record = ReadAllRows().FirstOrDefault(r => r.OrderId == orderId);
            if (record == null)
            {
                throw new InvalidOperationException($"Order {orderId} was not found in dummydata.txt.");
            }

            return record;
        }

        List<PreShipmentCarbonDataDTO> GetPreShipmentCarbonBreakdownByDate(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                (startDate, endDate) = (endDate, startDate);
            }

            return ReadAllRows()
                .Where(r => DateOnly.FromDateTime(r.TimeStamp) >= startDate && DateOnly.FromDateTime(r.TimeStamp) <= endDate)
                .OrderBy(r => r.TimeStamp)
                .ToList();
        }

        private static List<PreShipmentCarbonDataDTO> ReadAllRows()
        {
            var candidatePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "DataSource", "dummydata.txt"),
                Path.Combine(AppContext.BaseDirectory, "DataSource", "dummydata.txt"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "DataSource", "dummydata.txt")
            };

            var dataPath = candidatePaths.FirstOrDefault(File.Exists);
            if (dataPath == null)
            {
                throw new FileNotFoundException("Could not locate dummydata.txt.");
            }

            var rows = new List<PreShipmentCarbonDataDTO>();
            foreach (var line in File.ReadLines(dataPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                var parts = line.Split('|');
                if (parts.Length != 5)
                {
                    continue;
                }

                rows.Add(new PreShipmentCarbonDataDTO
                {
                    OrderId = int.Parse(parts[0], CultureInfo.InvariantCulture),
                    TimeStamp = DateTime.Parse(parts[1], CultureInfo.InvariantCulture),
                    ProductCF = float.Parse(parts[2], CultureInfo.InvariantCulture),
                    StorageCF = float.Parse(parts[3], CultureInfo.InvariantCulture),
                    PackagingCF = float.Parse(parts[4], CultureInfo.InvariantCulture)
                });
            }

            return rows;
        }
    }
}