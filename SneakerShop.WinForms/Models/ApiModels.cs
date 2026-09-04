namespace SneakerShop.WinForms.Models
{
    public class AuthResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public int? UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } =
            string.Empty;
    }

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

        public string ReleaseType { get; set; } = string.Empty;

        public string AuthenticityStatus { get; set; } =
            string.Empty;

        public string BoxCode { get; set; } = string.Empty;

        public int ReorderLevel { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public string DisplayName =>
            $"{Name} | {Colorway} | Size {Size}";
    }

    public class ItemRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal Size { get; set; }

        public string Colorway { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string ReleaseType { get; set; } = "Regular";

        public string AuthenticityStatus { get; set; } =
            "Pending";

        public string BoxCode { get; set; } = string.Empty;

        public int ReorderLevel { get; set; } = 5;

        public string PerformedBy { get; set; } = string.Empty;
    }

    public class StockMovementRequest
    {
        public int ItemId { get; set; }

        public int Quantity { get; set; }

        public string ReferenceNumber { get; set; } =
            string.Empty;

        public string Notes { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;
    }

    public class InventoryTransactionRecord
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public Item? Item { get; set; }

        public string TransactionType { get; set; } =
            string.Empty;

        public int Quantity { get; set; }

        public string ReferenceNumber { get; set; } =
            string.Empty;

        public string Notes { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }

    public class CreateOrderRequest
    {
        public string CustomerName { get; set; } =
            string.Empty;

        public string ProcessedBy { get; set; } =
            string.Empty;

        public List<CreateOrderLineRequest> Lines { get; set; } =
            new();
    }

    public class CreateOrderLineRequest
    {
        public int ItemId { get; set; }

        public int Quantity { get; set; }
    }

    public class OrderResponse
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string ProcessedBy { get; set; } = string.Empty;

        public List<OrderLineResponse> Lines { get; set; } =
            new();
    }

    public class OrderLineResponse
    {
        public int Id { get; set; }

        public int ItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Colorway { get; set; } = string.Empty;

        public decimal Size { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Subtotal { get; set; }

        public int ReturnedQuantity { get; set; }

        public int ExchangedQuantity { get; set; }

        public int AvailableForReturnOrExchange { get; set; }

        public string DisplayName =>
            $"{ItemName} | Size {Size}";
    }

    public class OrderActionRequest
    {
        public string Reason { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;
    }

    public class ReturnOrderItemRequest
    {
        public int OrderLineId { get; set; }

        public int Quantity { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;
    }

    public class ExchangeOrderItemRequest
    {
        public int OrderLineId { get; set; }

        public int NewItemId { get; set; }

        public int Quantity { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;
    }

    public class DashboardSummary
    {
        public int TotalProducts { get; set; }

        public int TotalModels { get; set; }

        public int TotalBrands { get; set; }

        public int TotalStock { get; set; }

        public decimal InventoryValue { get; set; }

        public int LowStockCount { get; set; }

        public int OutOfStockCount { get; set; }

        public int VerifiedCount { get; set; }

        public int PendingAuthenticity { get; set; }

        public int TotalOrders { get; set; }

        public int CancelledOrders { get; set; }

        public decimal GrossSales { get; set; }

        public int NetUnitsSold { get; set; }

        public decimal? BestSellingSize { get; set; }

        public int BestSellingSizeUnits { get; set; }

        public string? BestSellingBrand { get; set; }

        public int BestSellingBrandUnits { get; set; }

        public int RegisteredUsers { get; set; }
    }

    public class RestockSuggestion
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Brand { get; set; } = string.Empty;

        public string Colorway { get; set; } = string.Empty;

        public decimal Size { get; set; }

        public int CurrentQuantity { get; set; }

        public int ReorderLevel { get; set; }

        public int UnitsSoldLast30Days { get; set; }

        public int TargetStock { get; set; }

        public int SuggestedOrderQuantity { get; set; }

        public string Urgency { get; set; } = string.Empty;
    }
}