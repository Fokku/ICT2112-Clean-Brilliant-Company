using System.Data;
namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface IInboundLogisticsGateway
    {
        Task Insert(string restockID, float distanceKm, float durationMin, float timeStamp);
        Task<DataTable> FindBy(string restockID);
        Task DeleteBy(string restockID);
    }
}
