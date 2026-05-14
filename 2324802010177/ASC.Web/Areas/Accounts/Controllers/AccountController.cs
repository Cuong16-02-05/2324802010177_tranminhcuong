using ASC.Model;
using ASC.Web.Areas.Accounts.Models;
using ASC.Web.Controllers;
using ASC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // ── Lab 5 Part II: SERVICE ENGINEERS ─────────────────────────────

        [HttpGet]
        [Authorize(Roles = Constants.Roles.Admin)]
        public async Task<IActionResult> ServiceEngineers()
        {
            var engineers = await _userManager.GetUsersInRoleAsync(Constants.Roles.Engineer);
            var vm = new ServiceEngineerViewModel
            {
                ServiceEngineers = engineers.OrderBy(e => e.FirstName).ToList(),
                Registration = new ServiceEngineerRegistrationViewModel()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.Roles.Admin)]
        public async Task<IActionResult> ServiceEngineers(ServiceEngineerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ServiceEngineers = (await _userManager.GetUsersInRoleAsync(Constants.Roles.Engineer))
                    .OrderBy(e => e.FirstName).ToList();
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Registration.Email);
            if (existingUser == null)
            {
                // Tạo mới
                var newUser = new ApplicationUser
                {
                    UserName = model.Registration.Email,
                    Email = model.Registration.Email,
                    FirstName = model.Registration.FirstName,
                    IsActive = model.Registration.IsActive,
                    CreatedDate = DateTime.UtcNow,
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(newUser, model.Registration.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(newUser, Constants.Roles.Engineer);
                    await _emailSender.SendEmailAsync(
                        model.Registration.Email,
                        "Tài khoản Service Engineer đã được tạo - ASC",
                        $"<h3>Xin chào {model.Registration.FirstName}!</h3>" +
                        $"<p>Tài khoản của bạn đã được tạo thành công.</p>" +
                        $"<p>Email: <strong>{model.Registration.Email}</strong></p>");
                    TempData["Success"] = $"Đã tạo tài khoản engineer: {model.Registration.Email}";
                }
                else
                {
                    foreach (var err in result.Errors)
                        ModelState.AddModelError("", err.Description);
                    model.ServiceEngineers = (await _userManager.GetUsersInRoleAsync(Constants.Roles.Engineer))
                        .OrderBy(e => e.FirstName).ToList();
                    return View(model);
                }
            }
            else
            {
                // Cập nhật
                existingUser.FirstName = model.Registration.FirstName;
                existingUser.IsActive = model.Registration.IsActive;
                await _userManager.UpdateAsync(existingUser);
                await _emailSender.SendEmailAsync(
                    existingUser.Email!,
                    "Thông tin tài khoản đã cập nhật - ASC",
                    $"<h3>Xin chào {existingUser.FirstName}!</h3>" +
                    $"<p>Thông tin tài khoản của bạn đã được cập nhật.</p>" +
                    $"<p>Trạng thái: <strong>{(existingUser.IsActive ? "Active" : "Inactive")}</strong></p>");
                TempData["Success"] = $"Đã cập nhật: {existingUser.Email}";
            }

            return RedirectToAction(nameof(ServiceEngineers));
        }

        // ── Lab 5 Part III: CUSTOMERS ─────────────────────────────────────

        [HttpGet]
        [Authorize(Roles = Constants.Roles.Admin)]
        public async Task<IActionResult> Customers()
        {
            var customers = await _userManager.GetUsersInRoleAsync(Constants.Roles.User);
            var vm = new CustomerViewModel
            {
                Customers = customers.OrderBy(c => c.Email).ToList(),
                Registration = new CustomerRegistrationViewModel()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.Roles.Admin)]
        public async Task<IActionResult> Customers(CustomerViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Registration.Email);
            if (user != null)
            {
                user.IsActive = model.Registration.IsActive;
                await _userManager.UpdateAsync(user);
                await _emailSender.SendEmailAsync(
                    user.Email!,
                    "Cập nhật trạng thái tài khoản - ASC",
                    $"<h3>Xin chào!</h3>" +
                    $"<p>Trạng thái tài khoản của bạn đã được cập nhật.</p>" +
                    $"<p>Trạng thái: <strong>{(user.IsActive ? "Active" : "Inactive")}</strong></p>");
                TempData["Success"] = $"Đã cập nhật trạng thái: {user.Email}";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy user.";
            }
            return RedirectToAction(nameof(Customers));
        }

        // ── Lab 5 Part IV: PROFILE ────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return View(new ProfileViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                TempData["Success"] = "Cập nhật profile thành công!";
            else
                TempData["Error"] = "Cập nhật thất bại: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction(nameof(Profile));
        }
    }
}
