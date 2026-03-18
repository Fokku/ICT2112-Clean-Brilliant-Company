using CleanBrilliant.Domain.Entity;
namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IOSRMService
    {
        Task<float> GetRouteDistance(double longitude, double latitude);
    }
}
