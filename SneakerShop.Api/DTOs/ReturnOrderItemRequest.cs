using System.ComponentModel.DataAnnotations;

namespace SneakerShop.Api.DTOs
{
    public class ReturnOrderItemRequest
    {
        [Range(1, int.MaxValue)]
        public int OrderLineId { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public string PerformedBy { get; set; } =
            string.Empty;
    }
}