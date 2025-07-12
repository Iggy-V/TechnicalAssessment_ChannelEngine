namespace TechnicalAssessment_ChannelEngine.Models
{
    public class Product
    {
            public int Id { get; set; }
            public string Gtin { get; set; }
            public string Description { get; set; }
            public int Quantity { get; set; }

            public string MerchantProductId { get; set; }

           public int StockLocationId { get; set; }

    }

}