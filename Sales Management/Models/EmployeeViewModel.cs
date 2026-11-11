using System.ComponentModel.DataAnnotations;

namespace SalesManagement.Web.Models
{
    public class EmployeeViewModel
    {
        public int EmployeeId { get; set; }
        public int UserId { get; set; }

        [Display(Name = "Họ và tên")]
        public string? FullName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Chức danh")]
        public string? Position { get; set; }

        [Display(Name = "Phòng ban")]
        public string? Department { get; set; }

        [Display(Name = "Lương cơ bản")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public decimal? BasicSalary { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        public DateOnly? StartWorkingDate { get; set; }

        [Display(Name = "Loại hợp đồng")]
        public string? ContractType { get; set; }

        [Display(Name = "Tình trạng xóa")]
        public bool IsDeleted { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string? Username { get; set; }

        [Display(Name = "Vai trò")]
        public string? Role { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }

        public List<TimeAttendanceViewModel> TimeAttendances { get; set; } = new();
    }

    public class TimeAttendanceViewModel
    {
        public int AttendanceId { get; set; }
        public DateOnly Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? Status { get; set; }
        public double WorkHours { get; set; }
    }
}
