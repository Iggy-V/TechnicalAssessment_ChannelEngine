// Services/ChannelEngineService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Text;
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

            //_httpClient.DefaultRequestHeaders.Add("apikey", _apiKey);

        }

        public async Task<IEnumerable<Order>> GetOrdersInProgressAsync()
        {
            try
            {
                Console.WriteLine($"API Key: {_apiKey}, Base URL: {_baseUrl}");

                var response = await _httpClient.GetAsync($"{_baseUrl}/v2/orders?statuses=IN_PROGRESS&apikey={_apiKey}");
                response.EnsureSuccessStatusCode();
                //Console.WriteLine(response);
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);

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
                            Lines = new List<Product>()
                        };

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

                                order.Lines.Add(product);
                            }
                        }

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

    }
}