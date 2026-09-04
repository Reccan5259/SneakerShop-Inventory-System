using System.ComponentModel.DataAnnotations;

namespace SneakerShop.Api.DTOs
{
    public class CreateOrderRequest
    {
        [Required]
        [MinLength(2)]
        public string CustomerName { get; set; } =
            string.Empty;

        [Required]
        public string ProcessedBy { get; set; } =
            string.Empty;

        [Required]
        [MinLength(1)]
        public List<CreateOrderLineRequest> Lines
        {
            get;
            set;
        } = new();
    }

    public class CreateOrderLineRequest
    {
        [Range(1, int.MaxValue)]
        public int ItemId { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }
    }
}