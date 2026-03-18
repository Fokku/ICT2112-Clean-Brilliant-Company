namespace CleanBrilliant.Domain.DomainInterface
{
    public interface ICustomerDistanceService
    {
        Task<float> GetCustomerDistance(string orderID);
    }
}
