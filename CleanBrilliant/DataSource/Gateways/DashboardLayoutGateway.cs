using CleanBrilliant.Interfaces;
using Npgsql;
using System.Data;

namespace CleanBrilliant.DataSource.Gateways
{
    public class DashboardLayoutGateway : IDashboardLayoutGateway
    {
        private readonly string _connString;

        public DashboardLayoutGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
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

        public async Task<NpgsqlDataReader> GetAll()
        {
            const string sql = @"
                SELECT ""layoutId"", ""layoutName"", ""isDefault"", ""gridWidgetConfig""
                FROM clean_brilliant_company.dashboard_layout
                ORDER BY ""layoutId"";";

            var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(sql, conn);
            // Returns the reader and automatically closes the connection when the reader is disposed!
            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection); 
        }

        public async Task<NpgsqlDataReader> GetById(int layoutId)
        {
            const string sql = @"
                SELECT ""layoutId"", ""layoutName"", ""isDefault"", ""gridWidgetConfig""
                FROM clean_brilliant_company.dashboard_layout
                WHERE ""layoutId"" = @id;";

            var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", layoutId);

            return await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
        }

        public async Task<int> SaveOrUpdate(int layoutId, string layoutName, bool isDefault, string gridWidgetConfigJson)
        {
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
            
            // Handle null JSON blobs safely
            if (string.IsNullOrEmpty(gridWidgetConfigJson))
                cmd.Parameters.AddWithValue("config", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("config", gridWidgetConfigJson);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteLayout(int layoutId)
        {
            const string sql = @"
                DELETE FROM clean_brilliant_company.dashboard_layout
                WHERE ""layoutId"" = @id;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("id", layoutId);

            return await cmd.ExecuteNonQueryAsync();
        }
    }
}