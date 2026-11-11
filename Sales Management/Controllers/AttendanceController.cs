using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.BLL.Interfaces;
using System;
using System.Threading.Tasks;

namespace SalesManagement.Web.Controllers
{
    /// <summary>
    /// AttendanceController – Trang báo cáo chấm công.
    /// Dành cho Admin và Sales.
    /// </summary>
    [Authorize(Roles = "Admin,Sales")]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>
        /// Báo cáo chấm công với filter ngày từ / đến và nhân viên.
        /// Mặc định: 7 ngày gần nhất.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(
            DateOnly? fromDate,
            DateOnly? toDate,
            int? employeeId)
        {
            // Mặc định 7 ngày gần nhất nếu không có filter
            var defaultFrom = DateOnly.FromDateTime(DateTime.Today.AddDays(-6));
            var defaultTo = DateOnly.FromDateTime(DateTime.Today);

            var from = fromDate ?? defaultFrom;
            var to = toDate ?? defaultTo;

            // Giới hạn không cho toDate < fromDate
            if (to < from) to = from;

            var records = await _attendanceService.GetAttendanceReport(from, to, employeeId);
            var employees = await _attendanceService.GetEmployeeDropdown();
            var chartData = await _attendanceService.GetDailyWorkHours(from, to, employeeId);

            ViewBag.Employees = employees;
            ViewBag.ChartData = chartData;
            ViewBag.FromDate = from;
            ViewBag.ToDate = to;
            ViewBag.SelectedEmployeeId = employeeId;

            return View(records);
        }
    }
}
