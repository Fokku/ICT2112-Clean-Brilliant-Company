using CleanBrilliant.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CleanBrilliant.Services
{
    public interface IPreShipmentCarbonReader
    {
        PreShipmentCarbonDataDTO GetPreShipmentCarbonData(int orderId)
        {
            const string sql = @"
                SELECT order_id, time_stamp, product_cf, storage_cf, packaging_cf
                FROM pre_shipment_carbon_data
                WHERE order_id = @orderId
                LIMIT 1;";

            using var conn = new NpgsqlConnection(GetConnectionString());
            conn.Open();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("orderId", orderId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                throw new InvalidOperationException($"Order {orderId} was not found in pre_shipment_carbon_data.");
            }

            return MapRow(reader);
        }

        List<PreShipmentCarbonDataDTO> GetPreShipmentCarbonBreakdownByDate(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                (startDate, endDate) = (endDate, startDate);
            }

            const string sql = @"
                SELECT order_id, time_stamp, product_cf, storage_cf, packaging_cf
                FROM pre_shipment_carbon_data
                WHERE time_stamp::date BETWEEN @startDate AND @endDate
                ORDER BY time_stamp;";

            var rows = new List<PreShipmentCarbonDataDTO>();

            using var conn = new NpgsqlConnection(GetConnectionString());
            conn.Open();

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("startDate", startDate);
            cmd.Parameters.AddWithValue("endDate", endDate);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                rows.Add(MapRow(reader));
            }

            return rows;
        }

        private static string GetConnectionString()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connString = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection.");
            }

            return connString;
        }

        private static PreShipmentCarbonDataDTO MapRow(NpgsqlDataReader reader)
        {
            return new PreShipmentCarbonDataDTO
            {
                OrderId = reader.GetInt32(0),
                TimeStamp = reader.GetDateTime(1),
                ProductCF = (float)reader.GetDecimal(2),
                StorageCF = (float)reader.GetDecimal(3),
                PackagingCF = (float)reader.GetDecimal(4)
            };
        }
    }
}