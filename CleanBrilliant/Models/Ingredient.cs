namespace CleanBrilliant.Models
{
    public class Ingredient
    {
        public required string Name { get; set; }
        public float Volume { get; set; }
        public float ToxicityScore { get; set; } 
    }
}