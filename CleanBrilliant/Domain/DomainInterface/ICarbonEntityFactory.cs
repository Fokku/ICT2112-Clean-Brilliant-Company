using CleanBrilliant.Domain.Entity;
using EntityRouteData = CleanBrilliant.Domain.Entity.RouteData;
namespace CleanBrilliant.Domain.DomainInterface
{
    public interface ICarbonEntityFactory
    {
        EntityRouteData CreateRouteData(string referenceID);
        DistanceCF CreateDistanceCF(string deliveryMethod);
        TotalCF CreateTotalCF(string deliveryMethod);
    }
}
