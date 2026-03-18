using EntityRouteData = CleanBrilliant.Domain.Entity.RouteData;
namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IEstimatedTimeService
    {
        Task<float> GetOrderEstimatedTime(string orderID, EntityRouteData routeData);
    }
}
