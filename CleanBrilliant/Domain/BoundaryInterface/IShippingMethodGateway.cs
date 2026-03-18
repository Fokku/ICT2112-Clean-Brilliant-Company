namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface IShippingMethodGateway
    {
        Task InsertShippingMethod(string orderID, string shippingMethod);
        Task UpdateShippingMethod(string orderID, string shippingMethod);
        Task<string?> FindShippingMethod(string orderID);
    }
}
