using System.Data;
namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface ISupplierCarbonDataGateway
    {
        Task Insert(string restockID, float carbonAmount, float timeStamp);
        Task<DataTable> FindBy(string restockID);
        Task<DataTable> FindAll();
        Task DeleteBy(string restockID);
    }
}
