namespace SneakerShop.Api.Models
{
    public class OrderLine
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        public int ItemId { get; set; }

        public Item? Item { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Subtotal { get; set; }

        public int ReturnedQuantity { get; set; }

        public int ExchangedQuantity { get; set; }
    }
}