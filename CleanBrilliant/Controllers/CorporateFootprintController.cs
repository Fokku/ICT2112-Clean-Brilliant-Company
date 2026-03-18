using CleanBrilliant.Domain.Control;
using CleanBrilliant.Domain.Entity;
using Microsoft.AspNetCore.Mvc;

namespace CleanBrilliant.Controllers
{
    [ApiController]
    [Route("api/corporate-footprint")]
    public class CorporateFootprintController : ControllerBase
    {
        private readonly EmployeeCarbonManager _employeeCarbonManager;
        private readonly BuildingCarbonManager _buildingCarbonManager;

        public CorporateFootprintController(
            EmployeeCarbonManager employeeCarbonManager,
            BuildingCarbonManager buildingCarbonManager)
        {
            _employeeCarbonManager = employeeCarbonManager;
            _buildingCarbonManager = buildingCarbonManager;
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _employeeCarbonManager.GetStaffList();
            return Ok(employees);
        }

        [HttpPost("employees")]
        public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeRequest request)
        {
            var employee = new Employee(0, request.WorkMode, request.TravelDistance, request.TransportMode, request.DaysInOffice);
            await _employeeCarbonManager.AddEmployee(employee);
            return Ok(new { message = "Employee added successfully" });
        }

        [HttpDelete("employees/{id:int}")]
        public async Task<IActionResult> RemoveEmployee([FromRoute] int id)
        {
            await _employeeCarbonManager.RemoveEmployee(id);
            return Ok(new { message = $"Employee {id} removed successfully" });
        }

        [HttpGet("employees/{id:int}/footprint")]
        public async Task<IActionResult> GetEmployeeFootprint([FromRoute] int id)
        {
            var employees = await _employeeCarbonManager.GetStaffList();
            var employee = employees.FirstOrDefault(e => e.EmployeeId == id);
            if (employee == null)
                return NotFound(new { message = $"Employee with ID {id} not found" });

            var footprint = _employeeCarbonManager.CalculateStaffFootprint(employee);
            return Ok(new { employeeId = id, footprint });
        }

        [HttpGet("buildings")]
        public async Task<IActionResult> GetBuildings()
        {
            var buildings = await _buildingCarbonManager.GetBuildingList();
            return Ok(buildings);
        }

        [HttpPost("buildings")]
        public async Task<IActionResult> AddBuilding([FromBody] AddBuildingRequest request)
        {
            var building = new Building(0, request.Address, request.SquareFoot, request.Electricity, request.Gas);
            await _buildingCarbonManager.AddBuilding(building);
            return Ok(new { message = "Building added successfully" });
        }

        [HttpDelete("buildings/{id:int}")]
        public async Task<IActionResult> RemoveBuilding([FromRoute] int id)
        {
            await _buildingCarbonManager.RemoveBuilding(id);
            return Ok(new { message = $"Building {id} removed successfully" });
        }

        [HttpGet("buildings/{id:int}/footprint")]
        public async Task<IActionResult> GetBuildingFootprint([FromRoute] int id)
        {
            var buildings = await _buildingCarbonManager.GetBuildingList();
            var building = buildings.FirstOrDefault(b => b.BuildingId == id);
            if (building == null)
                return NotFound(new { message = $"Building with ID {id} not found" });

            var footprint = _buildingCarbonManager.CalculateFacilityFootprint(building);
            return Ok(new { buildingId = id, footprint });
        }
    }

    public class AddEmployeeRequest
    {
        public string Name { get; set; } = "";
        public WorkMode WorkMode { get; set; }
        public double TravelDistance { get; set; }
        public TransportMode TransportMode { get; set; }
        public int DaysInOffice { get; set; }
    }

    public class AddBuildingRequest
    {
        public string Address { get; set; } = "";
        public double SquareFoot { get; set; }
        public double Electricity { get; set; }
        public double Gas { get; set; }
    }
}
