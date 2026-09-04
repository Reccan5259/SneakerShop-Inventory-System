using SneakerShop.WinForms.Models;

namespace SneakerShop.WinForms
{
    public static class UserSession
    {
        public static AuthResponse? CurrentUser { get; set; }

        public static bool IsLoggedIn =>
            CurrentUser?.Success == true;

        public static bool IsAdmin =>
            CurrentUser?.Role.Equals(
                "Admin",
                StringComparison.OrdinalIgnoreCase) == true;

        public static string Username =>
            CurrentUser?.Username ?? "System";

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}