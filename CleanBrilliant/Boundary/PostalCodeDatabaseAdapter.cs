using CleanBrilliant.Domain.DomainInterface;

namespace CleanBrilliant.Boundary
{
    public class PostalCodeDatabaseAdapter : IPostalService
    {
        public async Task<(double Longitude, double Latitude)> GetPostalConversion(string sourcePostal, string destPostal)
        {
            await Task.CompletedTask;
            return (103.8198, 1.3521); // Singapore default placeholder
        }
    }
}
