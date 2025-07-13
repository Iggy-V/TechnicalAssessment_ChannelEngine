using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using TechnicalAssessment_ChannelEngine.Models;
using Order = TechnicalAssessment_ChannelEngine.Models.Order;

namespace TechnicalAssessment_ChannelEngine.Services
{
    public class OrderService : IOrderClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public OrderService(HttpClient httpClient, IConfiguration config, IOptions<ChannelEngineKey> options)
        {
            _httpClient = httpClient;
            _baseUrl = config["ChannelEngine:BaseUrl"];
            _apiKey = options.Value.ApiKey;
        }

        public virtual async Task<IEnumerable<Order>> GetOrdersInProgressAsync()
        {
            try
            {
                //Console.WriteLine($"API Key: {_apiKey}, Base URL: {_baseUrl}");

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
                    //Console.WriteLine(line);
                    var product = new Product
                    {
                        Id = line.GetProperty("Id").GetInt32(),
                        Gtin = line.GetProperty("Gtin").GetString(),
                        Description = line.GetProperty("Description").GetString(),
                        Quantity = line.GetProperty("Quantity").GetInt32(),
                        MerchantProductId = line.GetProperty("MerchantProductNo").GetString(),
                        StockLocationId = line.GetProperty("StockLocation").GetProperty("Id").GetInt32()
                    };

                    products.Add(product);
                }
            }

            return products;
        }
        // Function to aggregate products from all orders in progress
        public async Task<IEnumerable<Product>> GetAggregatedProductsAsync()
        {
            var orders = await GetOrdersInProgressAsync();
            
            return SortProducts(orders);
        }

        public static IEnumerable<Product> SortProducts(IEnumerable<Order> orders)
        {
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
                            Quantity = product.Quantity,
                            MerchantProductId = product.MerchantProductId,
                            StockLocationId = product.StockLocationId
                        };
                    }
                }
            }
            // Dictionary --> sorted list
            var aggregated = productMap.Values
                .OrderByDescending(p => p.Quantity)
                .ToList();

            return aggregated;
        }


        public async Task UpdateStock(Product product, int newStock)
        {


            // GET before update for testing
            //string getUrl = $"{_baseUrl}/v2/offer/stock?skus={product.MerchantProductId}&stockLocationIds={product.StockLocationId}&apikey={_apiKey}";
            //var getBeforeResponse = await _httpClient.GetAsync(getUrl);

            //Console.WriteLine($"[BEFORE UPDATE] Current stock for {product.MerchantProductId}:");
            //if (getBeforeResponse.IsSuccessStatusCode)
            //{
            //    var beforeJson = await getBeforeResponse.Content.ReadAsStringAsync();
            //    Console.WriteLine(beforeJson);
            //}
            //else
            //{
            //    Console.WriteLine($"Failed to fetch stock before update. Status: {getBeforeResponse.StatusCode}");
            //}

            // PUT to update stock
            var updatePayload = new[]
            {
                new
                {
                    MerchantProductNo = product.MerchantProductId,
                    StockLocations = new[]
                    {
                        new
                        {
                            Stock = newStock,
                            StockLocationId = product.StockLocationId
                        }
                    }
                }
            };

            var putContent = new StringContent(JsonSerializer.Serialize(updatePayload), Encoding.UTF8, "application/json");
            var putRequest = new HttpRequestMessage(HttpMethod.Put, $"{_baseUrl}/v2/offer/stock?apikey={_apiKey}")
            {
                Content = putContent
            };

            var putResponse = await _httpClient.SendAsync(putRequest);
            if (putResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"[UPDATE SUCCESS] Stock for {product.MerchantProductId} updated to {newStock}.");
            }
            else
            {
                var error = await putResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[UPDATE FAILED] Status: {putResponse.StatusCode}, Error: {error}");
                return;
            }

            // GET after update
            //var getAfterResponse = await _httpClient.GetAsync(getUrl);

            //Console.WriteLine($"[AFTER UPDATE] Stock for {product.MerchantProductId}:");
            //if (getAfterResponse.IsSuccessStatusCode)
            //{
            //    var afterJson = await getAfterResponse.Content.ReadAsStringAsync();
            //    Console.WriteLine(afterJson);
            //}
            //else
            //{
            //    Console.WriteLine($"Failed to fetch stock after update. Status: {getAfterResponse.StatusCode}");
            //}
        }

    
    }
}