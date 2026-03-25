using CleanBrilliant.Domain.Entity;
namespace CleanBrilliant.Domain.DomainInterface
{
    public interface ITotalCarbonService
    {
        Task<float> GetOrderCarbon(string orderID, string countryCode, string postalCode, ICarbonEntityFactory factory);
        Task<float> GetSupplierOrderCarbon(string restockID, string postalCode, string countryCode, ICarbonEntityFactory factory);
        Task<List<float>> GetCarbonLogs();
    }
}
