using ASC.Model;
using ASC.Utilities;
using ASC.Web.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<ApplicationSettings> _options;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          UserManager<ApplicationUser> userManager,
                          IOptions<ApplicationSettings> options)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _options = options;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            // Clear existing external cookie
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var currentUser = new CurrentUser
                    {
                        Email = user.Email,
                        UserName = user.UserName,
                        IsAdmin = roles.Contains(ASC.Model.Constants.Roles.Admin),
                        IsEngineer = roles.Contains(ASC.Model.Constants.Roles.Engineer),
                        IsUser = roles.Contains(ASC.Model.Constants.Roles.User)
                    };
                    HttpContext.Session.SetSession("CurrentUser", currentUser);
                }
                return LocalRedirect("/ServiceRequests/Dashboard/Dashboard");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
            return Page();
        }
    }
}
