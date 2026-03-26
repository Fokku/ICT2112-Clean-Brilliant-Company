using System.Data;

namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface ISupplierLookupGateway
    {
        Task<DataTable> FindAll();
    }
}
