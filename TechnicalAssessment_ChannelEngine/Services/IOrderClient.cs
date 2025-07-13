using TechnicalAssessment_ChannelEngine.Models;

namespace TechnicalAssessment_ChannelEngine.Services
{
    public interface IOrderClient
    {
        Task<IEnumerable<Product>> GetAggregatedProductsAsync();
        Task<IEnumerable<Order>> GetOrdersInProgressAsync();
        Task UpdateStock(Product product, int stock);

    }
}
