using CleanBrilliant.Domain.Entity;
namespace CleanBrilliant.Domain.BoundaryInterface
{
    public interface IEmployeeRepository
    {
        Task Insert(Employee employee);
        Task<Employee?> FindBy(int employeeID);
        Task<List<Employee>> FindAll();
        Task DeleteBy(int employeeID);
        Task Update(Employee employee);
    }
}
