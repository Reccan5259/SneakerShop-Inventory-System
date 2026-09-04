using System.ComponentModel.DataAnnotations;

namespace SneakerShop.Api.DTOs
{
    public class StockMovementRequest
    {
        [Range(1, int.MaxValue)]
        public int ItemId { get; set; }

        [Range(1, 100000)]
        public int Quantity { get; set; }

        [Required]
        public string ReferenceNumber { get; set; } =
            string.Empty;

        public string Notes { get; set; } = string.Empty;

        [Required]
        public string PerformedBy { get; set; } =
            string.Empty;
    }
}