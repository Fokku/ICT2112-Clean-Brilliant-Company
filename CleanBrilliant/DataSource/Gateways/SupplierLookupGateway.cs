using CleanBrilliant.Domain.BoundaryInterface;
using Npgsql;
using System.Data;

namespace CleanBrilliant.Data.Gateways
{
    public class SupplierLookupGateway : ISupplierLookupGateway
    {
        private readonly string _connString;

        public SupplierLookupGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task<DataTable> FindAll()
        {
            const string sql = @"
                SELECT *
                FROM supplier
                ORDER BY supplier_id;";

            var ds = new DataSet();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds, "supplier");

            return ds.Tables["supplier"] ?? new DataTable();
        }
    }
}
