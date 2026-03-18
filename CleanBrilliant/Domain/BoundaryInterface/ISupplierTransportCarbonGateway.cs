using System.Data;
namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface ISupplierTransportCarbonGateway
    {
        Task Insert(string restockID, float distanceCarbon, float timeStamp);
        Task<DataTable> FindBy(string restockID);
        Task DeleteBy(string restockID);
    }
}
