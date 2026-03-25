using System;
using System.Collections.Generic;
using CleanBrilliant.Interfaces;
using CleanBrilliant.Models;
using CleanBrilliant.Data.Gateways;

namespace CleanBrilliant.Services
{
    public class ProductCalculator : IProductDetailWriter, IProductDetailReader
    {
        private readonly ProductDetailGateway _gateway;

        // The Gateway is injected into the Control class
        public ProductCalculator(ProductDetailGateway gateway)
        {
            _gateway = gateway;
        }

        // Matches IProductDetailWriter
        public ProductDetail CreateProductDetail(int productId, float volume, List<Ingredient> ingredientList)
        {
            // 1. Create the blank domain object
            var detail = new ProductDetail 
            { 
                ProductID = productId,
                CalculationDate = DateTime.Now
            };

            // 2. Ask the domain object to run its calculations
            detail.ToxicPercentage = detail.CalculateToxicPercentage(ingredientList, volume);
            detail.Carbon = detail.CalculateProductCarbon(detail.ToxicPercentage, volume);
            detail.EcoFriendly = detail.CheckEcoFriendly(detail.Carbon);

            // 3. Save it to the database via the Gateway
            // Note: .Wait() is used to keep the interface synchronous as per your UML
            _gateway.InsertOrUpdateAsync(detail).Wait();

            return detail;
        }

        // Matches IProductDetailReader
        public ProductDetail GetProductDetail(int productId)
        {
            return _gateway.GetByIdAsync(productId).Result;
        }

        // Matches IProductDetailReader
        public List<ProductDetail> GetProductDetails(List<int> productIds)
        {
            var detailsList = new List<ProductDetail>();
            
            foreach (var id in productIds)
            {
                var detail = GetProductDetail(id);
                if (detail != null)
                {
                    detailsList.Add(detail);
                }
            }
            
            return detailsList;
        }
    }
}