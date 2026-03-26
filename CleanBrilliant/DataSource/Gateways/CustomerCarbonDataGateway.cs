using CleanBrilliant.Domain.BoundaryInterface;
using Npgsql;
using System.Data;

namespace CleanBrilliant.Data.Gateways
{
    public class CustomerCarbonDataGateway : ICustomerCarbonDataGateway
    {
        private readonly string _connString;

        public CustomerCarbonDataGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task Insert(string orderID, float carbonAmount, float timeStamp)
        {
            const string sql = @"
                INSERT INTO customer_carbon_data (order_id, carbon_amount, timestamp)
                VALUES (@orderID, @carbonAmount, @timestamp);";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);
            cmd.Parameters.AddWithValue("carbonAmount", carbonAmount);
            cmd.Parameters.AddWithValue("timestamp", timeStamp);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> FindBy(string orderID)
        {
            const string sql = @"
                SELECT order_id, carbon_amount, timestamp
                FROM customer_carbon_data
                WHERE order_id = @orderID
                ORDER BY timestamp DESC
                LIMIT 1;";

            var ds = new DataSet();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds, "customer_carbon_data");

            return ds.Tables["customer_carbon_data"] ?? new DataTable();
        }

        public async Task<DataTable> FindAll()
        {
            const string sql = @"
                SELECT order_id, carbon_amount, timestamp
                FROM customer_carbon_data
                ORDER BY order_id;";

            var ds = new DataSet();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds, "customer_carbon_data");

            return ds.Tables["customer_carbon_data"] ?? new DataTable();
        }

        public async Task DeleteBy(string orderID)
        {
            const string sql = @"
                DELETE FROM customer_carbon_data
                WHERE order_id = @orderID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
