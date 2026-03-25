namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IRestockDistanceService
    {
        Task<float> GetRestockDistance(string restockID);
    }
}
