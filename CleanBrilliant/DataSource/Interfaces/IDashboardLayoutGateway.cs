using Npgsql;

namespace CleanBrilliant.Interfaces
{
    public interface IDashboardLayoutGateway
    {
        Task<bool> AnyLayoutsExist();
        Task<NpgsqlDataReader> GetAll();
        Task<NpgsqlDataReader> GetById(int layoutId);
        Task<int> SaveOrUpdate(int layoutId, string layoutName, bool isDefault, string gridWidgetConfigJson);
        Task<int> DeleteLayout(int layoutId);
    }
}