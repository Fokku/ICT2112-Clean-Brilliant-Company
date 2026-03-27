using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CleanBrilliant.Models;
using CleanBrilliant.Data; 
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CleanBrilliant.Data.Gateways
{
    public class ProductDetailGateway 
    {
        private readonly string _connString;
        private readonly UnitOfWork _unitOfWork; 

        public ProductDetailGateway(IConfiguration config, UnitOfWork unitOfWork)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> InsertOrUpdateAsync(ProductDetail productDetail)
        {
            _unitOfWork.registerDirty(productDetail);
            
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
            // ... (parameters added as usual)
            
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<RecordSet> GetByIdAsync(int productId)
        {
            var recordSet = new RecordSet();
            const string sql = @"
                SELECT product_id, carbon, eco_friendly, toxic_percentage, calculation_date
                FROM product_detail
                WHERE product_id = @productId;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("productId", productId);

            await using var reader = await cmd.ExecuteReaderAsync();
            for (int i = 0; i < reader.FieldCount; i++) recordSet.Columns.Add(reader.GetName(i));

            if (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                recordSet.Rows.Add(row);
            }

            return recordSet; 
        }
    }
}