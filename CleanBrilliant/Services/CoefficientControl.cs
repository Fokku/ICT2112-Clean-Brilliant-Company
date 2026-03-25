using System.Collections.Generic;
using System.Linq;
using CleanBrilliant.Interfaces;

namespace CleanBrilliant.Services
{
    public class CoefficientControl : ICoefficientManager
    {
        private List<EmissionSource> coefficients;

        public CoefficientControl()
        {
            coefficients = new List<EmissionSource>
            {
                new TruckEmission(1.0f),
                new ShipEmission(1.0f),
                new PlaneEmission(1.0f),
                new TrainEmission(1.0f)
            };
        }

        public List<EmissionSource> getAll()
        {
            return coefficients;
        }

        public float getEmission(string name)
        {
            var source = coefficients.FirstOrDefault(c => c.GetName() == name);

            if (source == null)
                throw new KeyNotFoundException($"Emission type '{name}' not found.");

            return source.getEmission();
        }

        public void updateEmission(string name, float emission)
        {
            if (emission < 0)
            {
                throw new ArgumentException("Emission cannot be negative.");
            }

            if (emission > 1000) // optional upper bound
            {
                throw new ArgumentException("Emission value too large.");
            }

            var source = coefficients.FirstOrDefault(c => c.GetName() == name);

            if (source == null)
                throw new KeyNotFoundException($"Emission type '{name}' not found.");

            source.setEmission(emission);
        }
    }
}