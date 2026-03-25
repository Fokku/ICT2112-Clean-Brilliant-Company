using CleanBrilliant.Domain.BoundaryInterface;
using Npgsql;

namespace CleanBrilliant.Data.Gateways
{
    public class ShippingMethodGateway : IShippingMethodGateway
    {
        private readonly string _connString;

        public ShippingMethodGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task InsertShippingMethod(string orderID, string shippingMethod)
        {
            const string sql = @"
                INSERT INTO shipping_method (order_id, shipping_method)
                VALUES (@orderID, @shippingMethod);";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);
            cmd.Parameters.AddWithValue("shippingMethod", shippingMethod);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateShippingMethod(string orderID, string shippingMethod)
        {
            const string sql = @"
                UPDATE shipping_method
                SET shipping_method = @shippingMethod
                WHERE order_id = @orderID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);
            cmd.Parameters.AddWithValue("shippingMethod", shippingMethod);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<string?> FindShippingMethod(string orderID)
        {
            const string sql = @"
                SELECT shipping_method
                FROM shipping_method
                WHERE order_id = @orderID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderID", orderID);

            var result = await cmd.ExecuteScalarAsync();
            return result as string;
        }
    }
}
