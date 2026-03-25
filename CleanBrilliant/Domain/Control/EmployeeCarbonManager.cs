using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.Domain.Entity;

namespace CleanBrilliant.Domain.Control
{
    public class EmployeeCarbonManager : ICorpCarbonService
    {
        private readonly IEmployeeRepository _employeeRepository;

        private static readonly Dictionary<TransportMode, double> EmissionFactors = new()
        {
            { TransportMode.CAR, 0.21 },
            { TransportMode.BUS, 0.089 },
            { TransportMode.TRAIN, 0.041 }
        };

        public EmployeeCarbonManager(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task AddEmployee(Employee employee)
        {
            await _employeeRepository.Insert(employee);
        }

        public async Task RemoveEmployee(int employeeID)
        {
            await _employeeRepository.DeleteBy(employeeID);
        }

        public double CalculateStaffFootprint(Employee employee)
        {
            if (employee.EmployeeWorkmode == WorkMode.REMOTE)
                return 0;

            if (!EmissionFactors.TryGetValue(employee.TransportMode, out double factor))
                factor = EmissionFactors[TransportMode.CAR];

            return employee.TravelDistance * 2 * employee.DaysInOffice * 52 * factor;
        }

        public async Task<List<Employee>> GetStaffList()
        {
            return await _employeeRepository.FindAll();
        }

        public async Task<List<double>> GetEmployeeCarbonDataTotals()
        {
            var employees = await _employeeRepository.FindAll();
            var results = new List<double>();
            foreach (var employee in employees)
            {
                results.Add(CalculateStaffFootprint(employee));
            }
            return results;
        }

        public async Task<List<double>> GetBuildingCarbonDataTotals()
        {
            return await Task.FromResult(new List<double>());
        }
    }
}
