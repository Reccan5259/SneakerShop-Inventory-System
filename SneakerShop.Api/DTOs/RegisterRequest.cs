using System.ComponentModel.DataAnnotations;

namespace SneakerShop.Api.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [MinLength(3)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^[a-zA-Z0-9_]{4,20}$",
            ErrorMessage =
                "Username must contain 4 to 20 letters, numbers, or underscores.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
            ErrorMessage =
                "Password must contain at least 8 characters, uppercase, lowercase, number, and symbol.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(
            nameof(Password),
            ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}