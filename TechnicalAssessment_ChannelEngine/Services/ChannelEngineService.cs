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

                var response = await _httpClient.GetAsync($"{_baseUrl}/v2/orders?apikey={_apiKey}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var ids = new List<Order>();

                using (JsonDocument doc = JsonDocument.Parse(content))
                {
                    var root = doc.RootElement;
                    var contentArray = root.GetProperty("Content");

                    foreach (var item in contentArray.EnumerateArray())
                    {
                        var id = item.GetProperty("Id").GetInt32();

                        ids.Add(new Order { Id = id }); // Other props can be omitted
                    }
                }

                return ids;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting orders: {ex.Message}");
                return new List<Order>();
            }
        }

    }
}