using CleanBrilliant.Domain.Entity;
namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface IBuildingRepository
    {
        Task Insert(Building building);
        Task<Building?> FindBy(int buildingID);
        Task<List<Building>> FindAll();
        Task DeleteBy(int buildingID);
        Task Update(Building building);
    }
}
