namespace SneakerShop.Api.DTOs
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
}