using CleanBrilliant.Domain.Entity;
namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IOSRMService
    {
        Task<float> GetRouteDistance(double srcLongitude, double srcLatitude, double destLongitude, double destLatitude);
    }
}
