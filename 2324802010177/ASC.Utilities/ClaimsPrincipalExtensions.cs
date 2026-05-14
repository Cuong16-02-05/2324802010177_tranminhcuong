using System.Security.Claims;

namespace ASC.Utilities
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetCurrentUserEmail(this ClaimsPrincipal principal)
            => principal.FindFirstValue(ClaimTypes.Email);

        public static string? GetCurrentUserName(this ClaimsPrincipal principal)
            => principal.FindFirstValue(ClaimTypes.Name);

        public static bool IsAdmin(this ClaimsPrincipal principal) => principal.IsInRole("Admin");
        public static bool IsEngineer(this ClaimsPrincipal principal) => principal.IsInRole("Engineer");
        public static bool IsUser(this ClaimsPrincipal principal) => principal.IsInRole("User");
    }
}
