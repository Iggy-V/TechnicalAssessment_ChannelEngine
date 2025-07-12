using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TechnicalAssessment_ChannelEngine.Models;

namespace TechnicalAssessment_ChannelEngine.Services
{
    public class ChannelEngineService : ChannelEngineInterface
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public ChannelEngineService(HttpClient httpClient, IConfiguration configuration, IOptions<ChannelEngineKey> options)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
            _baseUrl = configuration["ChannelEngine:BaseUrl"];
        }

        public async Task<IEnumerable<Order>> GetOrdersInProgressAsync()
        {
            try
            {
                Console.WriteLine($"API Key: {_apiKey}, Base URL: {_baseUrl}");

                var response = await _httpClient.GetAsync($"{_baseUrl}/v2/orders?statuses=IN_PROGRESS&apikey={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var orders = new List<Order>();

                using (JsonDocument doc = JsonDocument.Parse(content))
                {
                    var root = doc.RootElement;
                    var contentArray = root.GetProperty("Content");

                    foreach (var orderElement in contentArray.EnumerateArray())
                    {
                        var order = new Order
                        {
                            Id = orderElement.GetProperty("Id").GetInt32(),
                            Status = orderElement.GetProperty("Status").GetString(),
                            Lines = ExtractProductsFromOrder(orderElement)
                        };

                        orders.Add(order);
                    }
                }

                return orders;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting orders: {ex.Message}");
                return new List<Order>();
            }
        }

        /// Function to extract individual prodcts from an order and creates a list of products for each order
        private List<Product> ExtractProductsFromOrder(JsonElement orderElement)
        {
            var products = new List<Product>();

            if (orderElement.TryGetProperty("Lines", out var linesElement))
            {
                foreach (var line in linesElement.EnumerateArray())
                {
                    var product = new Product
                    {
                        Id = line.GetProperty("Id").GetInt32(),
                        Gtin = line.GetProperty("Gtin").GetString(),
                        Description = line.GetProperty("Description").GetString(),
                        Quantity = line.GetProperty("Quantity").GetInt32()
                    };

                    products.Add(product);
                }
            }

            return products;
        }

        public async Task<IEnumerable<Product>> GetAggregatedProductsAsync()
        {
            var orders = await GetOrdersInProgressAsync();
            var productMap = new Dictionary<string, Product>();

            foreach (var order in orders)
            {
                foreach (var product in order.Lines)
                {
                    if (string.IsNullOrEmpty(product.Gtin))
                        continue;

                    if (productMap.TryGetValue(product.Gtin, out var existing))
                    {
                        existing.Quantity += product.Quantity;
                    }
                    else
                    {
                        productMap[product.Gtin] = new Product
                        {
                            Id = product.Id,
                            Gtin = product.Gtin,
                            Description = product.Description,
                            Quantity = product.Quantity
                        };
                    }
                }
            }

            var aggregated = productMap.Values
                .OrderByDescending(p => p.Quantity)
                .ToList();

            // Output to console
            Console.WriteLine("Aggregated Products (Descending by Quantity):");
            foreach (var p in aggregated)
            {
                Console.WriteLine($"GTIN: {p.Gtin}, Description: {p.Description}, Quantity: {p.Quantity}");
            }

            return aggregated;
        }
    }
}
