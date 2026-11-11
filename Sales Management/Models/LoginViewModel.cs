using System.ComponentModel.DataAnnotations;

namespace SalesManagement.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui l�ng nh?p t�n dang nh?p")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui l�ng nh?p m?t kh?u")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
