namespace SneakerShop.Api.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Completed";

        public string ProcessedBy { get; set; } = string.Empty;

        public List<OrderLine> Lines { get; set; } = new();
    }
}