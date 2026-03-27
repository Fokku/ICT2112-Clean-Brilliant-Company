namespace CleanBrilliant.Domain.DomainInterface
{
    public interface ISaveShippingMethod
    {
        Task SaveShippingMethod(string orderID, string shippingMethod);
    }
}
