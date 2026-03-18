using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.Entity;
using Npgsql;

namespace CleanBrilliant.Data.Gateways
{
    public class BuildingRepositoryGateway : IBuildingRepository
    {
        private readonly string _connString;

        public BuildingRepositoryGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task Insert(Building building)
        {
            const string sql = @"
                INSERT INTO building (address, square_foot, electricity_kwh, gas_volume)
                VALUES (@address, @squareFoot, @electricityKwh, @gasVolume);";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("address", building.Address);
            cmd.Parameters.AddWithValue("squareFoot", building.SquareFoot);
            cmd.Parameters.AddWithValue("electricityKwh", building.ElectricityKWH);
            cmd.Parameters.AddWithValue("gasVolume", building.GasVolume);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Building?> FindBy(int buildingID)
        {
            const string sql = @"
                SELECT building_id, address, square_foot, electricity_kwh, gas_volume
                FROM building
                WHERE building_id = @buildingID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("buildingID", buildingID);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Building(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDouble(2),
                    reader.GetDouble(3),
                    reader.GetDouble(4)
                );
            }

            return null;
        }

        public async Task<List<Building>> FindAll()
        {
            const string sql = @"
                SELECT building_id, address, square_foot, electricity_kwh, gas_volume
                FROM building
                ORDER BY building_id;";

            var buildings = new List<Building>();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                buildings.Add(new Building(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDouble(2),
                    reader.GetDouble(3),
                    reader.GetDouble(4)
                ));
            }

            return buildings;
        }

        public async Task DeleteBy(int buildingID)
        {
            const string sql = @"
                DELETE FROM building
                WHERE building_id = @buildingID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("buildingID", buildingID);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Update(Building building)
        {
            const string sql = @"
                UPDATE building
                SET address = @address,
                    square_foot = @squareFoot,
                    electricity_kwh = @electricityKwh,
                    gas_volume = @gasVolume
                WHERE building_id = @buildingID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("address", building.Address);
            cmd.Parameters.AddWithValue("squareFoot", building.SquareFoot);
            cmd.Parameters.AddWithValue("electricityKwh", building.ElectricityKWH);
            cmd.Parameters.AddWithValue("gasVolume", building.GasVolume);
            cmd.Parameters.AddWithValue("buildingID", building.BuildingId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
