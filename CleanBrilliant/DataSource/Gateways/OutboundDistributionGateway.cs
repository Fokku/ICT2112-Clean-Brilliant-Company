using CleanBrilliant.Domain.BoundaryInterface;
using Npgsql;
using System.Data;

namespace CleanBrilliant.Data.Gateways
{
    public class OutboundDistributionGateway : IOutboundDistributionGateway
    {
        private readonly string _connString;

        public OutboundDistributionGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task Insert(string orderID, string customerRouteDistID, float distanceKm, float durationMin)
        {
            const string sql = @"
                INSERT INTO outbound_distribution (order_id, customer_route_dist_id, distance_km, duration_min, timestamp)
                VALUES (@orderID, @customerRouteDistID, @distanceKm, @durationMin, @timestamp);";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);
            cmd.Parameters.AddWithValue("customerRouteDistID", customerRouteDistID);
            cmd.Parameters.AddWithValue("distanceKm", distanceKm);
            cmd.Parameters.AddWithValue("durationMin", durationMin);
            cmd.Parameters.AddWithValue("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> FindBy(string orderID)
        {
            const string sql = @"
                SELECT order_id, customer_route_dist_id, distance_km, duration_min, timestamp
                FROM outbound_distribution
                WHERE order_id = @orderID;";

            var ds = new DataSet();

            var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds, "outbound_distribution");

            return ds.Tables["outbound_distribution"] ?? new DataTable();
        }

        public async Task DeleteBy(string orderID)
        {
            const string sql = @"
                DELETE FROM outbound_distribution
                WHERE order_id = @orderID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
