using CleanBrilliant.Models;

namespace CleanBrilliant.Services
{
    public interface IAggregatedData
    {
        List<CO2AggregatePoint> GetAggregatedData(
            ComponentType componentType,
            AggregationType aggregation,
            DateOnly startDate,
            DateOnly endDate);
    }
}