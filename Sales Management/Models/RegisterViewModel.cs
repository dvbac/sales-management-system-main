using System.ComponentModel.DataAnnotations;

namespace SalesManagement.Web.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui l�ng nh?p t�n dang nh?p")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "T�n dang nh?p ph?i t? 3-50 k� t?")]
        [Display(Name = "T�n dang nh?p")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui l�ng nh?p email")]
        [EmailAddress(ErrorMessage = "Email kh�ng h?p l?")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui l�ng nh?p m?t kh?u")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "M?t kh?u ph?i t? 6-100 k� t?")]
        [DataType(DataType.Password)]
        [Display(Name = "M?t kh?u")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Vui l�ng x�c nh?n m?t kh?u")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "M?t kh?u kh�ng kh?p")]
        [Display(Name = "X�c nh?n m?t kh?u")]
        public string ConfirmPassword { get; set; } = null!;

        [StringLength(100)]
        [Display(Name = "H? v� t�n")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "S? di?n tho?i kh�ng h?p l?")]
        [Display(Name = "S? di?n tho?i")]
        public string? PhoneNumber { get; set; }
    }
}
