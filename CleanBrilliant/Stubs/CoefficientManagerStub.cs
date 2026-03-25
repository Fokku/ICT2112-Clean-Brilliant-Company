using CleanBrilliant.Domain.DomainInterface;
using CleanBrilliant.DTO;

namespace CleanBrilliant.Stubs
{
    public class CoefficientManagerStub : ICoefficientManager
    {
        private CoefficientDTO _coefficient = new()
        {
            ShippingMethod = "truck",
            CoefficientValue = 0.5f
        };

        public async Task<CoefficientDTO> GetCoefficient()
        {
            await Task.CompletedTask;
            return _coefficient;
        }

        public async Task UpdateCoefficient(CoefficientDTO coefficient)
        {
            await Task.CompletedTask;
            _coefficient = coefficient;
        }
    }
}
