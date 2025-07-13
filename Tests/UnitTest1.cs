using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechnicalAssessment_ChannelEngine.Models;
using TechnicalAssessment_ChannelEngine.Services;
using Xunit;

public class ChannelEngineServiceTests
{
    [Fact]
    public async Task GetTopProductsAsync_ReturnsTopNProducts()
    {
        // LIST has to be in descending order of Quantity as that is done by the a different service
        var products = new List<Product>
        {
            
            new Product
            {
                Id = 1,
                Gtin = "A",
                Description = "Product A",
                Quantity = 25,
                MerchantProductId = "A-001",
                StockLocationId = 1001
            },
            new Product
            {
                Id = 3,
                Gtin = "B",
                Description = "Product B",
                Quantity = 20,
                MerchantProductId = "B-001",
                StockLocationId = 1002
            },
            new Product
            {
                Id = 4,
                Gtin = "C",
                Description = "Product C",
                Quantity = 10,
                MerchantProductId = "C-001",
                StockLocationId = 1003
            },
            new Product
            {
                Id = 5,
                Gtin = "D",
                Description = "Product D",
                Quantity = 5,
                MerchantProductId = "D-001",
                StockLocationId = 1004
            },
            new Product
            {
                Id = 6,
                Gtin = "E",
                Description = "Product E",
                Quantity = 1,
                MerchantProductId = "E-001",
                StockLocationId = 1005
            }
        };

        var mockOrderClient = new Mock<IOrderClient>();
        mockOrderClient.Setup(x => x.GetAggregatedProductsAsync()).ReturnsAsync(products);

        var service = new ChannelEngineService(mockOrderClient.Object);

        // Act
        var topProducts = (await service.GetTopProductsAsync()).ToList();

        // Debug print
        //Console.WriteLine(string.Join("\n", topProducts.Select(p => $"{p.Gtin} - {p.Quantity}")));

        // Assert
        Assert.Equal(5, topProducts.Count);
        Assert.Equal("A", topProducts[0].Gtin);
        Assert.Equal(25, topProducts[0].Quantity);
        Assert.Equal("B", topProducts[1].Gtin);
        Assert.Equal(20, topProducts[1].Quantity);
    }



    // ...

    [Fact]
    public async Task SortProductsAsync_AggregatesQuantitiesCorrectly()
    {
        var orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                Lines = new List<Product>
                {
                    new Product { Gtin = "A", Description = "Prod A", Quantity = 10, MerchantProductId = "A-001", StockLocationId = 1 },
                    new Product { Gtin = "B", Description = "Prod B", Quantity = 5, MerchantProductId = "B-001", StockLocationId = 2 }
                }
            },
            new Order
            {
                Id = 2,
                Lines = new List<Product>
                {
                    new Product { Gtin = "A", Description = "Prod A again", Quantity = 15, MerchantProductId = "A-001", StockLocationId = 1 },
                    new Product { Gtin = "C", Description = "Prod A again", Quantity = 1, MerchantProductId = "A-001", StockLocationId = 1 },
                    new Product { Gtin = "D", Description = "Prod A again", Quantity = 2, MerchantProductId = "A-001", StockLocationId = 1 }
                }
            }
        };


        // IMPROVE CREATE FAKE SERVICE
        var httpClient = new HttpClient();
        var configuration = new ConfigurationBuilder().Build();
        var options = Options.Create(new ChannelEngineKey());

        var orderService = new OrderService(httpClient, configuration, options);
        var aggregatedProducts = await OrderService.SortProductsAsync(orders, orderService);

        // Assert
        Assert.Equal(4, aggregatedProducts.Count());

        var productA = aggregatedProducts.First(p => p.Gtin == "A");
        var productB = aggregatedProducts.First(p => p.Gtin == "B");

        Assert.Equal(25, productA.Quantity);
        Assert.Equal(5, productB.Quantity);
    }

}