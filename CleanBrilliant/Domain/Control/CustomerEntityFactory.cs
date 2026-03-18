using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.Domain.Entity;
using EntityRouteData = CleanBrilliant.Domain.Entity.RouteData;

namespace CleanBrilliant.Domain.Control
{
    public class CustomerEntityFactory : ICarbonEntityFactory
    {
        public EntityRouteData CreateRouteData(string referenceID)
            => new CustomerRouteData { OrderId = referenceID };

        public DistanceCF CreateDistanceCF(string deliveryMethod)
            => new OrderDistanceCF { DeliveryMethod = deliveryMethod };

        public TotalCF CreateTotalCF(string deliveryMethod)
            => new TotalCustomerCF { DeliveryMethod = deliveryMethod };
    }
}
