using System.Data;
using Npgsql;

namespace CleanBrilliant.Data.Gateways
{
    public class PreShipmentGateway
    {
        private readonly string _connString;

        public PreShipmentGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
        }

        public DataTable FindByDateRange(string tableName, DateTime startDate, DateTime endDate)
        {
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();

            string sql = $"SELECT order_id, time_stamp, product_cf, storage_cf, packaging_cf, shipment_cf FROM {tableName} WHERE time_stamp >= @start AND time_stamp <= @end ORDER BY time_stamp;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("start", startDate);
            cmd.Parameters.AddWithValue("end", endDate);

            using var reader = cmd.ExecuteReader();
            var dataTable = new DataTable();
            dataTable.Load(reader);
            
            return dataTable;
        }

        public DataTable FindByOrder(string tableName, int orderId)
        {
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();

            string sql = $"SELECT order_id, time_stamp, product_cf, storage_cf, packaging_cf, shipment_cf FROM {tableName} WHERE order_id = @id ORDER BY time_stamp DESC LIMIT 1;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", orderId);

            using var reader = cmd.ExecuteReader();
            var dataTable = new DataTable();
            dataTable.Load(reader);
            
            return dataTable;
        }

        public void Insert(string tableName, int orderId, DateTime timeStamp, float productCf, float storageCf, float packagingCf, float shipmentCf)
        {
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();

            string sql = $"INSERT INTO {tableName} (order_id, time_stamp, product_cf, storage_cf, packaging_cf, shipment_cf) VALUES (@id, @ts, @pCf, @sCf, @pkgCf, @shipCf);";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", orderId);
            cmd.Parameters.AddWithValue("ts", timeStamp);
            cmd.Parameters.AddWithValue("pCf", productCf);
            cmd.Parameters.AddWithValue("sCf", storageCf);
            cmd.Parameters.AddWithValue("pkgCf", packagingCf);
            cmd.Parameters.AddWithValue("shipCf", shipmentCf);

            cmd.ExecuteNonQuery();
        }

        public void Update(string tableName, int orderId, DateTime timeStamp, float productCf, float storageCf, float packagingCf, float shipmentCf)
        {
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();

            string sql = $"UPDATE {tableName} SET time_stamp = @ts, product_cf = @pCf, storage_cf = @sCf, packaging_cf = @pkgCf, shipment_cf = @shipCf WHERE order_id = @id;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", orderId);
            cmd.Parameters.AddWithValue("ts", timeStamp);
            cmd.Parameters.AddWithValue("pCf", productCf);
            cmd.Parameters.AddWithValue("sCf", storageCf);
            cmd.Parameters.AddWithValue("pkgCf", packagingCf);
            cmd.Parameters.AddWithValue("shipCf", shipmentCf);

            cmd.ExecuteNonQuery();
        }

        public void Delete(string tableName, int orderId)
        {
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();

            string sql = $"DELETE FROM {tableName} WHERE order_id = @id;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", orderId);

            cmd.ExecuteNonQuery();
        }
    }
}