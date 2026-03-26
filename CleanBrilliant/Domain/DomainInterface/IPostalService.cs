namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IPostalService
    {
        Task<(double Latitude, double Longitude)?> GetCoordinates(string postalCode);
    }
}
