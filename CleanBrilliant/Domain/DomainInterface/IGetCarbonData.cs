namespace CleanBrilliant.Domain.DomainInterface
{
    public interface IGetCarbonData
    {
        Task<float> GetOrderShippingCarbon(string orderID);
        Task<float> GetRestockShippingCarbon(string restockID);
    }
}
