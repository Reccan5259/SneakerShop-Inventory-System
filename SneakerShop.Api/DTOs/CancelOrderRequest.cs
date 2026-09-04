using System.ComponentModel.DataAnnotations;

namespace SneakerShop.Api.DTOs
{
    public class CancelOrderRequest
    {
        [Required]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public string PerformedBy { get; set; } =
            string.Empty;
    }
}