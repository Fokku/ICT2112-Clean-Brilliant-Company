namespace CleanBrilliant.Domain.DomainInterface
{
    public interface ICarbonAnalysisService
    {
        Task<string> GetShippingRecommendation(string postalCode, string deliveryType, string countryCode);
    }
}
