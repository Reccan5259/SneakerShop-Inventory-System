namespace SneakerShop.Api.Models
{
    public class InventoryTransaction
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public Item? Item { get; set; }

        public string TransactionType { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public string ReferenceNumber { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}