namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IPostalService
    {
        Task<(double Longitude, double Latitude)> GetPostalConversion(string sourcePostal, string destPostal);
    }
}
