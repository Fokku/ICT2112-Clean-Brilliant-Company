using System.Data;
namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface IOutboundDistributionGateway
    {
        Task Insert(string orderID, string customerRouteDistID, float distanceKm, float durationMin);
        Task<DataTable> FindBy(string orderID);
        Task<DataTable> FindAll();
        Task DeleteBy(string orderID);
    }
}
