using System.Data;
namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface ICustomerCarbonDataGateway
    {
        Task Insert(string orderID, float carbonAmount, float timeStamp);
        Task<DataTable> FindBy(string orderID);
        Task<DataTable> FindAll();
        Task DeleteBy(string orderID);
    }
}
