using CleanBrilliant.Domain.BoundaryInterface;
using Npgsql;
using System.Data;

namespace CleanBrilliant.Data.Gateways
{
    public class SupplierTransportCarbonGateway : ISupplierTransportCarbonGateway
    {
        private readonly string _connString;

        public SupplierTransportCarbonGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task Insert(string restockID, float distanceCarbon, float timeStamp)
        {
            const string sql = @"
                INSERT INTO supplier_transport_carbon (restock_id, carbon_amount, timestamp)
                VALUES (@restockID, @distanceCarbon, @timestamp);";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("restockID", restockID);
            cmd.Parameters.AddWithValue("distanceCarbon", distanceCarbon);
            cmd.Parameters.AddWithValue("timestamp", timeStamp);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> FindBy(string restockID)
        {
            const string sql = @"
                SELECT restock_id, shipping_method, carbon_amount, timestamp
                FROM supplier_transport_carbon
                WHERE restock_id = @restockID
                ORDER BY timestamp DESC
                LIMIT 1;";

            var ds = new DataSet();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("restockID", restockID);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds, "supplier_transport_carbon");

            return ds.Tables["supplier_transport_carbon"] ?? new DataTable();
        }

        public async Task DeleteBy(string restockID)
        {
            const string sql = @"
                DELETE FROM supplier_transport_carbon
                WHERE restock_id = @restockID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("restockID", restockID);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
