using CleanBrilliant.Models;
using CleanBrilliant.Interfaces;

namespace CleanBrilliant.Services
{
    public class Co2AnalyticsControl : IAggregatedData, IPreShipmentCarbonReader
    {
        public List<CO2AggregatePoint> GetAggregatedData(
            ComponentType componentType,
            AggregationType aggregation,
            DateOnly startDate,
            DateOnly endDate)
        {
            var rows = ((IPreShipmentCarbonReader)this).GetPreShipmentCarbonBreakdownByDate(startDate, endDate);

            return rows
                .GroupBy(r => DateOnly.FromDateTime(r.TimeStamp))
                .OrderBy(g => g.Key)
                .Select(g => new CO2AggregatePoint
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Value = CalculateAggregate(g.Select(r => SelectComponentValue(r, componentType)), aggregation)
                })
                .ToList();
        }

        private static double SelectComponentValue(PreShipmentCarbonDataDTO row, ComponentType componentType)
        {
            return componentType switch
            {
                ComponentType.PRODUCT => row.ProductCF,
                ComponentType.STORAGE => row.StorageCF,
                ComponentType.PACKAGING => row.PackagingCF,
                ComponentType.SHIPMENT => row.ProductCF + row.StorageCF + row.PackagingCF,
                ComponentType.TOTAL => row.ProductCF + row.StorageCF + row.PackagingCF,
                _ => 0
            };
        }

        private static double CalculateAggregate(IEnumerable<double> values, AggregationType aggregation)
        {
            var points = values as double[] ?? values.ToArray();
            if (points.Length == 0)
            {
                return 0;
            }

            return aggregation switch
            {
                AggregationType.SUM => points.Sum(),
                AggregationType.AVG => points.Average(),
                AggregationType.MIN => points.Min(),
                AggregationType.MAX => points.Max(),
                _ => points.Sum()
            };
        }
    }
}