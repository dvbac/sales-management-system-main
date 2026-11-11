using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalesManagement.BLL.DTOs;

namespace SalesManagement.BLL.Interfaces
{
    public interface IAttendanceService
    {
        /// <summary>Lấy danh sách chấm công theo ngày</summary>
        Task<IEnumerable<AttendanceReportDto>> GetAttendanceByDate(DateOnly date);

        /// <summary>Lấy danh sách chấm công theo nhân viên</summary>
        Task<IEnumerable<AttendanceReportDto>> GetAttendanceByUser(int employeeId);

        /// <summary>Lấy báo cáo tổng hợp với filter ngày + nhân viên (nullable)</summary>
        Task<IEnumerable<AttendanceReportDto>> GetAttendanceReport(DateOnly? fromDate, DateOnly? toDate, int? employeeId);

        /// <summary>Lấy danh sách nhân viên để populate dropdown filter</summary>
        Task<IEnumerable<EmployeeDropdownDto>> GetEmployeeDropdown();

        /// <summary>Tính tổng giờ làm theo ngày để hiển thị chart</summary>
        Task<IEnumerable<DailyWorkHoursDto>> GetDailyWorkHours(DateOnly fromDate, DateOnly toDate, int? employeeId);
    }
}
