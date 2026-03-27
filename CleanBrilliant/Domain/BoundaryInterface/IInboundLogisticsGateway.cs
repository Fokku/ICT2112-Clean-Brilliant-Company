using System.Data;
namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface IInboundLogisticsGateway
    {
        Task Insert(string restockID, string supplierRouteDistID, float distanceKm, float durationMin, float timeStamp);
        Task<DataTable> FindBy(string restockID);
        Task<DataTable> FindAll();
        Task DeleteBy(string restockID);
    }
}
