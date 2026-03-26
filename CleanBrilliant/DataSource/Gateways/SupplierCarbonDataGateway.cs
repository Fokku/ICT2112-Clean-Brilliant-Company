using CleanBrilliant.Domain.BoundaryInterface;
using Npgsql;
using System.Data;

namespace CleanBrilliant.Data.Gateways
{
    public class SupplierCarbonDataGateway : ISupplierCarbonDataGateway
    {
        private readonly string _connString;

        public SupplierCarbonDataGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task Insert(string restockID, float carbonAmount, float timeStamp)
        {
            const string sql = @"
                INSERT INTO supplier_carbon_data (restock_id, carbon_amount, timestamp)
                VALUES (@restockID, @carbonAmount, @timestamp);";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("restockID", restockID);
            cmd.Parameters.AddWithValue("carbonAmount", carbonAmount);
            cmd.Parameters.AddWithValue("timestamp", timeStamp);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> FindBy(string restockID)
        {
            const string sql = @"
                SELECT restock_id, carbon_amount, timestamp
                FROM supplier_carbon_data
                WHERE restock_id = @restockID
                ORDER BY timestamp DESC
                LIMIT 1;";

            var ds = new DataSet();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("restockID", restockID);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds, "supplier_carbon_data");

            return ds.Tables["supplier_carbon_data"] ?? new DataTable();
        }

        public async Task<DataTable> FindAll()
        {
            const string sql = @"
                SELECT restock_id, carbon_amount, timestamp
                FROM supplier_carbon_data
                ORDER BY restock_id;";

            var ds = new DataSet();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds, "supplier_carbon_data");

            return ds.Tables["supplier_carbon_data"] ?? new DataTable();
        }

        public async Task DeleteBy(string restockID)
        {
            const string sql = @"
                DELETE FROM supplier_carbon_data
                WHERE restock_id = @restockID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("restockID", restockID);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
