using System;
using System.Collections.Generic;
using CleanBrilliant.Data; 
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CleanBrilliant.Data.Gateways
{
    public class PreShipmentGateway
    {
        private readonly string _connString;
        private readonly UnitOfWork _unitOfWork; 

        public PreShipmentGateway(IConfiguration config, UnitOfWork unitOfWork)
        {
            _connString = config.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
            _unitOfWork = unitOfWork;
        }

        public RecordSet FindByDateRange(string tableName, DateTime startDate, DateTime endDate)
        {
            var recordSet = new RecordSet();
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();

            string sql = $"SELECT order_id, time_stamp, product_cf, storage_cf, packaging_cf FROM {tableName} WHERE time_stamp >= @start AND time_stamp <= @end ORDER BY time_stamp;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("start", startDate);
            cmd.Parameters.AddWithValue("end", endDate);

            using var reader = cmd.ExecuteReader();
            for (int i = 0; i < reader.FieldCount; i++) recordSet.Columns.Add(reader.GetName(i));

            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                recordSet.Rows.Add(row);
            }

            return recordSet; 
        }

        public RecordSet FindByOrder(string tableName, int orderId)
        {
            var recordSet = new RecordSet();
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();

            string sql = $"SELECT order_id, time_stamp, product_cf, storage_cf, packaging_cf FROM {tableName} WHERE order_id = @id ORDER BY time_stamp DESC LIMIT 1;";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", orderId);

            using var reader = cmd.ExecuteReader();
            for (int i = 0; i < reader.FieldCount; i++) recordSet.Columns.Add(reader.GetName(i));

            if (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                recordSet.Rows.Add(row);
            }

            return recordSet; 
        }

        public void Insert(string tableName, int orderId, DateTime timeStamp, float productCf, float storageCf, float packagingCf)
        {
            _unitOfWork.registerNew(new { OrderId = orderId, Table = tableName });
            
            using var conn = new NpgsqlConnection(_connString);
            conn.Open();
            string sql = $"INSERT INTO {tableName} (order_id, time_stamp, product_cf, storage_cf, packaging_cf) VALUES (@id, @ts, @pCf, @sCf, @pkgCf);";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", orderId);
            cmd.Parameters.AddWithValue("ts", timeStamp);
            cmd.Parameters.AddWithValue("pCf", productCf);
            cmd.Parameters.AddWithValue("sCf", storageCf);
            cmd.Parameters.AddWithValue("pkgCf", packagingCf);
            cmd.ExecuteNonQuery();
        }

        public void Update(string tableName, int orderId, DateTime timeStamp, float productCf, float storageCf, float packagingCf)
        {
            _unitOfWork.registerDirty(new { OrderId = orderId, Table = tableName });

            using var conn = new NpgsqlConnection(_connString);
            conn.Open();
            string sql = $"UPDATE {tableName} SET time_stamp = @ts, product_cf = @pCf, storage_cf = @sCf, packaging_cf = @pkgCf WHERE order_id = @id;";
            using var cmd = new NpgsqlCommand(sql, conn);
            // ... (parameters added as usual)
            cmd.ExecuteNonQuery();
        }

        public void Delete(string tableName, int orderId)
        {
            _unitOfWork.registerDeleted(new { OrderId = orderId, Table = tableName });
            // ... (execute raw SQL deletion as usual)
        }
    }
}