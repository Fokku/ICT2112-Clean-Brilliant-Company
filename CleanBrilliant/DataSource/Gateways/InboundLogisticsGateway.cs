using CleanBrilliant.Domain.BoundaryInterface;
using Npgsql;
using System.Data;

namespace CleanBrilliant.Data.Gateways
{
    public class InboundLogisticsGateway : IInboundLogisticsGateway
    {
        private readonly string _connString;

        public InboundLogisticsGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task Insert(string restockID, string supplierRouteDistID, float distanceKm, float durationMin, float timeStamp)
        {
            const string sql = @"
                INSERT INTO inbound_logistics (restock_id, supplier_route_dist_id, distance_km, duration_min, timestamp)
                VALUES (@restockID, @supplierRouteDistID, @distanceKm, @durationMin, @timestamp);";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("restockID", restockID);
            cmd.Parameters.AddWithValue("supplierRouteDistID", supplierRouteDistID);
            cmd.Parameters.AddWithValue("distanceKm", distanceKm);
            cmd.Parameters.AddWithValue("durationMin", durationMin);
            cmd.Parameters.AddWithValue("timestamp", timeStamp);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> FindBy(string restockID)
        {
            const string sql = @"
                SELECT restock_id, supplier_route_dist_id, distance_km, duration_min, timestamp
                FROM inbound_logistics
                WHERE restock_id = @restockID
                ORDER BY timestamp DESC
                LIMIT 1;";

            var ds = new DataSet();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("restockID", restockID);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds, "inbound_logistics");

            return ds.Tables["inbound_logistics"] ?? new DataTable();
        }

        public async Task<DataTable> FindAll()
        {
            const string sql = @"
                SELECT restock_id, supplier_route_dist_id, distance_km, duration_min, timestamp
                FROM inbound_logistics
                ORDER BY timestamp DESC, restock_id ASC;";

            var ds = new DataSet();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);

            using var adapter = new NpgsqlDataAdapter(cmd);
            adapter.Fill(ds, "inbound_logistics");

            return ds.Tables["inbound_logistics"] ?? new DataTable();
        }

        public async Task DeleteBy(string restockID)
        {
            const string sql = @"
                DELETE FROM inbound_logistics
                WHERE restock_id = @restockID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("restockID", restockID);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
