using ASC.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ASC.Web.Areas.Identity.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ProviderDisplayName { get; set; }
        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback",
                values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(
            string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });

            // Lấy email từ Google
            var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? "";

            // Kiểm tra nếu email là Admin hoặc Engineer → báo lỗi
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                var roles = await _userManager.GetRolesAsync(existingUser);
                if (roles.Contains(ASC.Model.Constants.Roles.Admin) ||
                    roles.Contains(ASC.Model.Constants.Roles.Engineer))
                {
                    ErrorMessage = "Tài khoản Admin hoặc Engineer không thể đăng nhập qua Google.";
                    return RedirectToPage("./Login");
                }
            }

            // Thử đăng nhập bằng external login
            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey,
                isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
                return LocalRedirect("/ServiceRequests/Dashboard/Dashboard");

            // Chưa có tài khoản → tự động tạo tài khoản User mới
            if (!string.IsNullOrEmpty(email))
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Tạo tài khoản mới với role User
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "",
                        LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        EmailConfirmed = true
                    };
                    var createResult = await _userManager.CreateAsync(user);
                    if (createResult.Succeeded)
                        await _userManager.AddToRoleAsync(user, ASC.Model.Constants.Roles.User);
                }

                var existingLogins = await _userManager.GetLoginsAsync(user);
                var alreadyLinked = existingLogins.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey);
                if (!alreadyLinked)
                    await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect("/ServiceRequests/Dashboard/Dashboard");
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, ASC.Model.Constants.Roles.User);
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect("/ServiceRequests/Dashboard/Dashboard");
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
