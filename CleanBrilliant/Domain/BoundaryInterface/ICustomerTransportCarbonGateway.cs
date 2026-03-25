using System.Data;
namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface ICustomerTransportCarbonGateway
    {
        Task Insert(string orderID, float distanceCarbon, float timeStamp);
        Task<DataTable> FindBy(string orderID);
        Task DeleteBy(string orderID);
    }
}
