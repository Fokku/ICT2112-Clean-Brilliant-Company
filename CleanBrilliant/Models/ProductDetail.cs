using System;
using System.Collections.Generic;
using System.Linq;

namespace CleanBrilliant.Models
{
    public class ProductDetail
    {
        public int ProductID { get; set; }
        public float Carbon { get; set; }
        public bool EcoFriendly { get; set; }
        public float ToxicPercentage { get; set; }
        public DateTime CalculationDate { get; set; }

        // UML: + calculateToxicPercentage(ingredientList : List<Ingredient>) : float
        public float CalculateToxicPercentage(List<Ingredient> ingredientList, float totalVolume)
        {
            if (totalVolume <= 0 || ingredientList == null || !ingredientList.Any()) 
                return 0f;

            // Sophisticated logic: Weighted toxicity based on ingredient volume
            float totalToxicity = ingredientList.Sum(i => i.ToxicityScore * i.Volume);
            return (totalToxicity / totalVolume) * 100f;
        }

        // UML: + calculateProductCarbon(toxicPercentage : float, volume : float) : float
       public float CalculateProductCarbon(float toxicPercentage, float volume)
        {
            // Assuming volume is in ml, we scale down the base factor. 
            // Let's say 1 ml produces 0.0005 kg of base carbon.
            float baseCarbonFactorPerMl = 0.0005f; 
            return (volume * baseCarbonFactorPerMl) * (1 + (toxicPercentage / 100f));
        }

        // UML: + checkEcoFriendly(carbon : float) : boolean
        public bool CheckEcoFriendly(float carbon)
        {
            // Threshold updated: Eco-Friendly if carbon is less than 250 grams (0.25 kg)
            // If your system's base unit is actually grams, change this to 250.0f
            return carbon < 0.25f;
        }
    }
}