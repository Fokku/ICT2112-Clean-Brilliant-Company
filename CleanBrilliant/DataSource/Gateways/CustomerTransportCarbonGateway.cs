using CleanBrilliant.Domain.BoundaryInterface;
using Npgsql;
using System.Data;

namespace CleanBrilliant.Data.Gateways
{
    public class CustomerTransportCarbonGateway : ICustomerTransportCarbonGateway
    {
        private readonly string _connString;

        public CustomerTransportCarbonGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task Insert(string orderID, float distanceCarbon, float timeStamp)
        {
            const string sql = @"
                INSERT INTO customer_transport_carbon (order_id, carbon_amount, timestamp)
                VALUES (@orderID, @distanceCarbon, @timestamp);";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);
            cmd.Parameters.AddWithValue("distanceCarbon", distanceCarbon);
            cmd.Parameters.AddWithValue("timestamp", timeStamp);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> FindBy(string orderID)
        {
            const string sql = @"
                SELECT order_id, shipping_method, carbon_amount, timestamp
                FROM customer_transport_carbon
                WHERE order_id = @orderID
                ORDER BY timestamp DESC
                LIMIT 1;";

            var ds = new DataSet();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds, "customer_transport_carbon");

            return ds.Tables["customer_transport_carbon"] ?? new DataTable();
        }

        public async Task DeleteBy(string orderID)
        {
            const string sql = @"
                DELETE FROM customer_transport_carbon
                WHERE order_id = @orderID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
