using System.Collections.Generic;
using CleanBrilliant.Models;

namespace CleanBrilliant.Interfaces
{
    public interface IProductDetailReader
    {
        ProductDetail GetProductDetail(int productId);
        List<ProductDetail> GetProductDetails(List<int> productIds);
    }
}