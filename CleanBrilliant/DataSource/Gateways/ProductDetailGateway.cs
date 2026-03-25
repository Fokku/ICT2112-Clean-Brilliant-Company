using System;
using System.Threading.Tasks;
using CleanBrilliant.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CleanBrilliant.DataSource.Gateways
{
    public class ProductDetailGateway 
    {
        private readonly string _connString;

        public ProductDetailGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task<bool> InsertOrUpdateAsync(ProductDetail productDetail)
        {
            const string sql = @"
                INSERT INTO product_detail (product_id, carbon, eco_friendly, toxic_percentage, calculation_date)
                VALUES (@productId, @carbon, @ecoFriendly, @toxicPercentage, @calculationDate)
                ON CONFLICT (product_id) 
                DO UPDATE SET 
                    carbon = EXCLUDED.carbon,
                    eco_friendly = EXCLUDED.eco_friendly,
                    toxic_percentage = EXCLUDED.toxic_percentage,
                    calculation_date = EXCLUDED.calculation_date;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("productId", productDetail.ProductID);
            cmd.Parameters.AddWithValue("carbon", productDetail.Carbon);
            cmd.Parameters.AddWithValue("ecoFriendly", productDetail.EcoFriendly);
            cmd.Parameters.AddWithValue("toxicPercentage", productDetail.ToxicPercentage);
            cmd.Parameters.AddWithValue("calculationDate", productDetail.CalculationDate);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<ProductDetail> GetByIdAsync(int productId)
        {
            const string sql = @"
                SELECT product_id, carbon, eco_friendly, toxic_percentage, calculation_date
                FROM product_detail
                WHERE product_id = @productId;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("productId", productId);

            await using var reader = await cmd.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new ProductDetail
                {
                    ProductID = reader.GetInt32(reader.GetOrdinal("product_id")),
                    Carbon = reader.GetFloat(reader.GetOrdinal("carbon")),
                    EcoFriendly = reader.GetBoolean(reader.GetOrdinal("eco_friendly")),
                    ToxicPercentage = reader.GetFloat(reader.GetOrdinal("toxic_percentage")),
                    CalculationDate = reader.GetDateTime(reader.GetOrdinal("calculation_date"))
                };
            }

            return null; 
        }
    }
}