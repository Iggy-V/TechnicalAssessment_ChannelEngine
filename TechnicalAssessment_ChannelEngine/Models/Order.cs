namespace TechnicalAssessment_ChannelEngine.Models
{
    /// Only having the necassary properties for this assignemnt there are more properties 
    /// API documentation for more detailed properties list
    public class Order
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public List<Product> Lines { get; set; } = new();

    }

}
