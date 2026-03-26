using CleanBrilliant.Domain.Entity;
namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IOSRMService
    {
        Task<(float DistanceKm, float DurationMin)> GetRoute(
            double sourceLongitude,
            double sourceLatitude,
            double destinationLongitude,
            double destinationLatitude);
    }
}
