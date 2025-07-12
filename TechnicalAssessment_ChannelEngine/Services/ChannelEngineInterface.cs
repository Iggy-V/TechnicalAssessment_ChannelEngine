using System.Text.Json;
using TechnicalAssessment_ChannelEngine.Models;

namespace TechnicalAssessment_ChannelEngine.Services
{
    public interface ChannelEngineInterface
    {
        Task<IEnumerable<Product>> GetAggregatedProductsAsync();
        Task<IEnumerable<Order>> GetOrdersInProgressAsync();
        
    }
}