using System.Text.Json;
using TechnicalAssessment_ChannelEngine.Models;

namespace TechnicalAssessment_ChannelEngine.Services
{
    public interface ChannelEngineInterface
    {
        Task<IEnumerable<Product>> GetTopProductsAsync(int count = 5);
        Task<IEnumerable<Product>> GetAggregatedProductsAsync();
        Task<IEnumerable<Order>> GetOrdersInProgressAsync();


    }
}