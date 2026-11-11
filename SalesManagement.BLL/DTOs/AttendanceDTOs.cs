using System;

namespace SalesManagement.BLL.DTOs
{
    /// <summary>Dữ liệu chấm công cho báo cáo</summary>
    public class AttendanceReportDto
    {
        public int AttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double WorkHours { get; set; }
        public string? Status { get; set; }
        public int MinutesLate { get; set; }
        public bool IsLate => MinutesLate > 0;
        public bool IsLeftEarly { get; set; }

        /// <summary>Thời gian ca bắt đầu (từ Shift)</summary>
        public TimeSpan? ShiftStartTime { get; set; }
        /// <summary>Thời gian ca kết thúc (từ Shift)</summary>
        public TimeSpan? ShiftEndTime { get; set; }
    }

    /// <summary>Dropdown nhân viên cho filter</summary>
    public class EmployeeDropdownDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    /// <summary>Giờ làm theo ngày (dùng cho chart.js)</summary>
    public class DailyWorkHoursDto
    {
        public DateOnly Date { get; set; }
        public double TotalWorkHours { get; set; }
        public int TotalEmployees { get; set; }
    }
}
