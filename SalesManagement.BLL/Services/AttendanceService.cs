using Microsoft.EntityFrameworkCore;
using SalesManagement.BLL.Interfaces;
using SalesManagement.BLL.DTOs;
using SalesManagement.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Services
{
    /// <summary>
    /// AttendanceService – báo cáo chấm công nhân viên.
    /// Tính giờ làm, phát hiện đi muộn / về sớm dựa trên Shift.
    /// </summary>
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;

        // Ngưỡng coi là "về sớm" (phút)
        private const int EarlyLeavThresholdMinutes = 10;

        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AttendanceReportDto>> GetAttendanceByDate(DateOnly date)
            => await GetAttendanceReport(date, date, null);

        /// <inheritdoc/>
        public async Task<IEnumerable<AttendanceReportDto>> GetAttendanceByUser(int employeeId)
            => await GetAttendanceReport(null, null, employeeId);

        /// <inheritdoc/>
        public async Task<IEnumerable<AttendanceReportDto>> GetAttendanceReport(
            DateOnly? fromDate, DateOnly? toDate, int? employeeId)
        {
            var query = _context.TimeAttendances
                .Include(a => a.Employee).ThenInclude(e => e.User)
                .Include(a => a.Shift)
                .AsNoTracking()
                .AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.Date >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(a => a.Date <= toDate.Value);
            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            var records = await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Employee.User.FullName)
                .ToListAsync();

            return records.Select(a => MapToDto(a));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EmployeeDropdownDto>> GetEmployeeDropdown()
        {
            return await _context.Employees
                .Include(e => e.User)
                .Where(e => !e.IsDeleted)
                .OrderBy(e => e.User.FullName)
                .Select(e => new EmployeeDropdownDto
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.User.FullName ?? e.User.Username,
                    Department = e.Department ?? ""
                })
                .AsNoTracking()
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DailyWorkHoursDto>> GetDailyWorkHours(
            DateOnly fromDate, DateOnly toDate, int? employeeId)
        {
            var query = _context.TimeAttendances
                .AsNoTracking()
                .Where(a => a.Date >= fromDate && a.Date <= toDate);

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            var grouped = await query
                .GroupBy(a => a.Date)
                .Select(g => new DailyWorkHoursDto
                {
                    Date = g.Key,
                    TotalWorkHours = g.Sum(a => a.WorkHours),
                    TotalEmployees = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return grouped;
        }

        // ─── Private helpers ─────────────────────────────────────────────────────────

        private static AttendanceReportDto MapToDto(DAL.Entities.TimeAttendance a)
        {
            var dto = new AttendanceReportDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee?.User?.FullName ?? a.Employee?.User?.Username ?? "N/A",
                Department = a.Employee?.Department ?? "",
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                WorkHours = a.WorkHours,
                Status = a.Status,
                MinutesLate = a.MinutesLate,
                ShiftStartTime = a.Shift?.StartTime,
                ShiftEndTime = a.Shift?.EndTime
            };

            // Phát hiện về sớm: so sánh giờ checkout với giờ kết thúc ca
            if (a.CheckOutTime.HasValue && a.Shift?.EndTime != null)
            {
                var checkOutTime = TimeOnly.FromDateTime(a.CheckOutTime.Value);
                var shiftEnd = TimeOnly.FromTimeSpan(a.Shift.EndTime);
                dto.IsLeftEarly = checkOutTime < shiftEnd.AddMinutes(-EarlyLeavThresholdMinutes);
            }

            return dto;
        }
    }
}
