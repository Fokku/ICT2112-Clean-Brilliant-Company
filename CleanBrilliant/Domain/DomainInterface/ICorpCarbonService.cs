namespace CleanBrilliant.Domain.DomainInterface
{
    public interface ICorpCarbonService
    {
        Task<List<double>> GetEmployeeCarbonDataTotals();
        Task<List<double>> GetBuildingCarbonDataTotals();
    }
}
