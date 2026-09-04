using System.ComponentModel.DataAnnotations;

namespace SneakerShop.Api.DTOs
{
    public class ItemRequest
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MinLength(3)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string Brand { get; set; } = string.Empty;

        [Range(0.01, 1000000)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100000)]
        public int Quantity { get; set; }

        [Range(1, 20)]
        public decimal Size { get; set; }

        [Required]
        public string Colorway { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string ReleaseType { get; set; } = "Regular";

        [Required]
        public string AuthenticityStatus { get; set; } = "Pending";

        [Required]
        public string BoxCode { get; set; } = string.Empty;

        [Range(0, 1000)]
        public int ReorderLevel { get; set; } = 5;

        public string PerformedBy { get; set; } = "System";
    }
}