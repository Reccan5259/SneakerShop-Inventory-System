namespace SneakerShop.Api.Models
{
    public class Item
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal Size { get; set; }

        public string Colorway { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string ReleaseType { get; set; } = "Regular";

        public string AuthenticityStatus { get; set; } = "Pending";

        public string BoxCode { get; set; } = string.Empty;

        public int ReorderLevel { get; set; } = 5;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}