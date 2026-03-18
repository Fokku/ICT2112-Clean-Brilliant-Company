using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.Domain.Entity;
using EntityRouteData = CleanBrilliant.Domain.Entity.RouteData;

namespace CleanBrilliant.Domain.Control
{
    public class SupplierEntityFactory : ICarbonEntityFactory
    {
        public EntityRouteData CreateRouteData(string referenceID)
            => new SupplierRouteData { RestockId = referenceID };

        public DistanceCF CreateDistanceCF(string deliveryMethod)
            => new SupplierDistanceCF { DeliveryMethod = deliveryMethod };

        public TotalCF CreateTotalCF(string deliveryMethod)
            => new TotalRestockCF { DeliveryMethod = deliveryMethod };
    }
}
