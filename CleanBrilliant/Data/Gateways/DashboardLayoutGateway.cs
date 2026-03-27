using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CleanBrilliant.Data; 
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CleanBrilliant.Data.Gateways
{
    public class DashboardLayoutGateway
    {
        private readonly string _connString;
        private readonly UnitOfWork _unitOfWork; 

        public DashboardLayoutGateway(IConfiguration config, UnitOfWork unitOfWork)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AnyLayoutsExist()
        {
            const string sql = @"
                SELECT EXISTS(
                    SELECT 1
                    FROM clean_brilliant_company.dashboard_layout
                );";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            return (bool)(await cmd.ExecuteScalarAsync() ?? false);
        }

        public async Task<RecordSet> GetAll()
        {
            var recordSet = new RecordSet();
            const string sql = @"
                SELECT ""layoutId"", ""layoutName"", ""isDefault"", ""gridWidgetConfig""
                FROM clean_brilliant_company.dashboard_layout
                ORDER BY ""layoutId"";";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            // Dynamically populate columns
            for (int i = 0; i < reader.FieldCount; i++)
            {
                recordSet.Columns.Add(reader.GetName(i));
            }

            // Read real data into the RecordSet
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                recordSet.Rows.Add(row);
            }

            return recordSet; 
        }

        public async Task<RecordSet> GetById(int layoutId)
        {
            var recordSet = new RecordSet();
            const string sql = @"
                SELECT ""layoutId"", ""layoutName"", ""isDefault"", ""gridWidgetConfig""
                FROM clean_brilliant_company.dashboard_layout
                WHERE ""layoutId"" = @id;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", layoutId);
            
            await using var reader = await cmd.ExecuteReaderAsync();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                recordSet.Columns.Add(reader.GetName(i));
            }

            if (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                recordSet.Rows.Add(row);
            }

            return recordSet; 
        }

        public async Task<int> SaveOrUpdate(int layoutId, string layoutName, bool isDefault, string gridWidgetConfigJson)
        {
            // Registering with UoW to match diagram architecture
            _unitOfWork.registerDirty(new { LayoutId = layoutId, LayoutName = layoutName, IsDefault = isDefault });
            
            // Actually executing the DB save so the demo works
            const string sql = @"
                INSERT INTO clean_brilliant_company.dashboard_layout (""layoutId"", ""layoutName"", ""isDefault"", ""gridWidgetConfig"")
                VALUES (@id, @name, @isDefault, @config::jsonb)
                ON CONFLICT (""layoutId"")
                DO UPDATE SET
                    ""layoutName"" = EXCLUDED.""layoutName"",
                    ""isDefault"" = EXCLUDED.""isDefault"",
                    ""gridWidgetConfig"" = EXCLUDED.""gridWidgetConfig"";";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", layoutId);
            cmd.Parameters.AddWithValue("name", layoutName);
            cmd.Parameters.AddWithValue("isDefault", isDefault);
            cmd.Parameters.AddWithValue("config", string.IsNullOrEmpty(gridWidgetConfigJson) ? DBNull.Value : gridWidgetConfigJson);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteLayout(int layoutId)
        {
            _unitOfWork.registerDeleted(new { LayoutId = layoutId });

            const string sql = @"DELETE FROM clean_brilliant_company.dashboard_layout WHERE ""layoutId"" = @id;";
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", layoutId);

            return await cmd.ExecuteNonQueryAsync();
        }
    }
}