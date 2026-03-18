using CleanBrilliant.Domain.BoundaryInterface;
using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.Domain.Entity;

namespace CleanBrilliant.Domain.Control
{
    public class BuildingCarbonManager : ICorpCarbonService
    {
        private readonly IBuildingRepository _buildingRepository;

        private const double ElectricityEmissionFactor = 0.4;  // kg CO2/kWh
        private const double GasEmissionFactor = 2.0;           // kg CO2/m³

        public BuildingCarbonManager(IBuildingRepository buildingRepository)
        {
            _buildingRepository = buildingRepository;
        }

        public async Task AddBuilding(Building building)
        {
            await _buildingRepository.Insert(building);
        }

        public async Task RemoveBuilding(int buildingID)
        {
            await _buildingRepository.DeleteBy(buildingID);
        }

        public double CalculateFacilityFootprint(Building building)
        {
            return building.ElectricityKWH * ElectricityEmissionFactor
                 + building.GasVolume * GasEmissionFactor;
        }

        public async Task<List<Building>> GetBuildingList()
        {
            return await _buildingRepository.FindAll();
        }

        public async Task<List<double>> GetBuildingCarbonDataTotals()
        {
            var buildings = await _buildingRepository.FindAll();
            var results = new List<double>();
            foreach (var building in buildings)
            {
                results.Add(CalculateFacilityFootprint(building));
            }
            return results;
        }

        public async Task<List<double>> GetEmployeeCarbonDataTotals()
        {
            return await Task.FromResult(new List<double>());
        }
    }
}
