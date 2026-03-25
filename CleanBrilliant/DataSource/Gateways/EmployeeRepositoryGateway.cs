using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.Entity;
using Npgsql;

namespace CleanBrilliant.Data.Gateways
{
    public class EmployeeRepositoryGateway : IEmployeeRepository
    {
        private readonly string _connString;

        public EmployeeRepositoryGateway(IConfiguration config)
        {
            _connString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in appsettings.json");
        }

        public async Task Insert(Employee employee)
        {
            const string sql = @"
                INSERT INTO employee (workmode, travel_distance, transport_mode, days_in_office)
                VALUES (@workmode, @travelDistance, @transportMode, @daysInOffice);";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("workmode", (int)employee.EmployeeWorkmode);
            cmd.Parameters.AddWithValue("travelDistance", employee.TravelDistance);
            cmd.Parameters.AddWithValue("transportMode", (int)employee.TransportMode);
            cmd.Parameters.AddWithValue("daysInOffice", employee.DaysInOffice);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Employee?> FindBy(int employeeID)
        {
            const string sql = @"
                SELECT employee_id, workmode, travel_distance, transport_mode, days_in_office
                FROM employee
                WHERE employee_id = @employeeID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("employeeID", employeeID);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Employee(
                    reader.GetInt32(0),
                    (WorkMode)reader.GetInt32(1),
                    reader.GetDouble(2),
                    (TransportMode)reader.GetInt32(3),
                    reader.GetInt32(4)
                );
            }

            return null;
        }

        public async Task<List<Employee>> FindAll()
        {
            const string sql = @"
                SELECT employee_id, workmode, travel_distance, transport_mode, days_in_office
                FROM employee
                ORDER BY employee_id;";

            var employees = new List<Employee>();

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                employees.Add(new Employee(
                    reader.GetInt32(0),
                    (WorkMode)reader.GetInt32(1),
                    reader.GetDouble(2),
                    (TransportMode)reader.GetInt32(3),
                    reader.GetInt32(4)
                ));
            }

            return employees;
        }

        public async Task DeleteBy(int employeeID)
        {
            const string sql = @"
                DELETE FROM employee
                WHERE employee_id = @employeeID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("employeeID", employeeID);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task Update(Employee employee)
        {
            const string sql = @"
                UPDATE employee
                SET workmode = @workmode,
                    travel_distance = @travelDistance,
                    transport_mode = @transportMode,
                    days_in_office = @daysInOffice
                WHERE employee_id = @employeeID;";

            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("workmode", (int)employee.EmployeeWorkmode);
            cmd.Parameters.AddWithValue("travelDistance", employee.TravelDistance);
            cmd.Parameters.AddWithValue("transportMode", (int)employee.TransportMode);
            cmd.Parameters.AddWithValue("daysInOffice", employee.DaysInOffice);
            cmd.Parameters.AddWithValue("employeeID", employee.EmployeeId);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
