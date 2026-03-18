namespace CleanBrilliant.Domain.Entity
{
    public abstract class CarbonSource
    {
        private double _totalCarbonFootprint;
        public double GetCarbonResult() => _totalCarbonFootprint;
        public void SetCarbonResult(double co2) => _totalCarbonFootprint = co2;
    }
}
