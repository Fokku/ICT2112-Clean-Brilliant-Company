using EntityRouteData = CleanBrilliant.Domain.Entity.RouteData;
namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IDistanceCarbonService
    {
        Task<float> GetDistanceCarbon(string orderID, EntityRouteData routeData);
        Task<float> CalculateCustomerDistanceCarbon(float distanceKm, string shippingMethod);
        Task<float> CalculateSupplierDistanceCarbon(float distanceKm, string shippingMethod);
    }
}
