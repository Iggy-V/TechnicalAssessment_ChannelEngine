using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using TechnicalAssessment_ChannelEngine.Models;

namespace TechnicalAssessment_ChannelEngine.Services
{
    public class ChannelEngineService : ChannelEngineInterface
    {
        private readonly IOrderClient _orderClient;

        public ChannelEngineService(IOrderClient orderClient)
        {
            _orderClient = orderClient;
        }


        // Filters top 5 products only and outputs them to the console
        public async Task<IEnumerable<Product>> GetTopProductsAsync(int count = 5)
        {
            var products = await _orderClient.GetAggregatedProductsAsync();

            var topProducts = products
                .Take(count)
                .ToList();


            // Print the top products to the console
            Console.WriteLine($"Top {count} Products (by Quantity):");
            foreach (var p in topProducts)
            {
                Console.WriteLine($"GTIN: {p.Gtin}, Description: {p.Description}, Quantity: {p.Quantity}, LocaitonID: {p.StockLocationId}");
            }

            return topProducts;
        }
    }
}
