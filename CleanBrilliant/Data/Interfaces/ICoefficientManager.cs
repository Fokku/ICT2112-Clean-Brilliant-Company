using CleanBrilliant.Models;

namespace CleanBrilliant.Interfaces
{
    public interface ICoefficientManager
    {

        public float getEmission(string name);

        public void updateEmission(string name, float emission);

    }
}