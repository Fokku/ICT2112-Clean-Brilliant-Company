using CleanBrilliant.DTO;
namespace CleanBrilliant.Domain.DomainInterface
{
    public interface ICoefficientManager
    {
        Task<CoefficientDTO> GetCoefficient();
        Task UpdateCoefficient(CoefficientDTO coefficient);
    }
}
