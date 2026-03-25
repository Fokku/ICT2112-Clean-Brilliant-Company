using System.Collections.Generic;
using CleanBrilliant.Models;

namespace CleanBrilliant.Interfaces
{
    public interface IProductDetailWriter
    {
        ProductDetail CreateProductDetail(int productId, float volume, List<Ingredient> ingredientList);
    }
}