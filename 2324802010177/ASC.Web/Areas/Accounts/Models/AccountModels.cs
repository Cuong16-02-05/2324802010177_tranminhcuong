using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.Accounts.Models
{
    // ── Lab 5 Part II: Service Engineer ViewModels ──────────────────────

    /// <summary>
    /// Lưu trữ thông tin đăng ký Service Engineer mới
    /// </summary>
    public class ServiceEngineerRegistrationViewModel
    {
        [Required(ErrorMessage = "Tên không được để trống")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự và có chứa số")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Hiển thị danh sách + form thêm/sửa Service Engineer
    /// </summary>
    public class ServiceEngineerViewModel
    {
        public List<ASC.Model.ApplicationUser> ServiceEngineers { get; set; } = new();
        public ServiceEngineerRegistrationViewModel Registration { get; set; } = new();
    }

    // ── Lab 5 Part III: Customer ViewModels ─────────────────────────────

    /// <summary>
    /// Lưu trữ thông tin khách hàng đăng ký
    /// </summary>
    public class CustomerRegistrationViewModel
    {
        public string? Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Danh sách khách hàng + form cập nhật
    /// </summary>
    public class CustomerViewModel
    {
        public List<ASC.Model.ApplicationUser> Customers { get; set; } = new();
        public CustomerRegistrationViewModel Registration { get; set; } = new();
    }

    // ── Lab 5 Part IV: Profile ViewModel ────────────────────────────────

    /// <summary>
    /// Cập nhật thông tin profile người dùng
    /// </summary>
    public class ProfileViewModel
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }
    }
}
