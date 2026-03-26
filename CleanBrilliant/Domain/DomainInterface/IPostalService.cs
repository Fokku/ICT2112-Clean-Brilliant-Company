namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IPostalService
    {
        Task<((double Longitude, double Latitude) Source, (double Longitude, double Latitude) Dest)> GetPostalConversion(string sourcePostal, string destPostal);
    }
}
