using System.Collections.Generic;

namespace CleanBrilliant.Services
{
    public interface ICoefficientManager
    {
        List<EmissionSource> getAll();
        float getEmission(string name);
        void updateEmission(string name, float emission);
    }
}