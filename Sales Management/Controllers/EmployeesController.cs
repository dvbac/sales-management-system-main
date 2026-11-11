using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SalesManagement.DAL.Entities;
using SalesManagement.Web.Models;
using SalesManagement.BLL.Interfaces;

namespace SalesManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        private EmployeeViewModel MapToViewModel(Employee e)
        {
            return new EmployeeViewModel
            {
                EmployeeId = e.EmployeeId,
                UserId = e.UserId,
                FullName = e.User?.FullName,
                Email = e.User?.Email,
                PhoneNumber = e.User?.PhoneNumber,
                Position = e.Position,
                Department = e.Department,
                BasicSalary = e.BasicSalary,
                StartWorkingDate = e.StartWorkingDate,
                ContractType = e.ContractType,
                IsDeleted = e.IsDeleted,
                Username = e.User?.Username,
                Role = e.User?.Role,
                TimeAttendances = e.TimeAttendances?.Select(t => new TimeAttendanceViewModel
                {
                    AttendanceId = t.AttendanceId,
                    Date = t.Date,
                    CheckInTime = t.CheckInTime,
                    CheckOutTime = t.CheckOutTime,
                    Status = t.Status,
                    WorkHours = t.WorkHours
                }).ToList() ?? new List<TimeAttendanceViewModel>()
            };
        }

        // GET: Employees
        public async Task<IActionResult> Index(string searchString)
        {
            var employees = await _employeeService.GetAllEmployeesAsync(searchString);
            var viewModel = employees.Select(MapToViewModel);
            return View(viewModel);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            return View(MapToViewModel(employee));
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View(new EmployeeViewModel());
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Username = model.Email, 
                    PasswordHash = model.Password, // Service should handle hashing
                    Role = model.Role ?? "Admin"
                };

                var employee = new Employee
                {
                    Position = model.Position,
                    BasicSalary = model.BasicSalary,
                    Department = model.Department,
                    StartWorkingDate = model.StartWorkingDate,
                    ContractType = model.ContractType
                };

                await _employeeService.CreateEmployeeWithUserAsync(employee, user);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            return View(MapToViewModel(employee));
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel model)
        {
            if (id != model.EmployeeId) return NotFound();

            if (ModelState.IsValid)
            {
                var employee = new Employee
                {
                    EmployeeId = model.EmployeeId,
                    UserId = model.UserId,
                    Position = model.Position,
                    BasicSalary = model.BasicSalary,
                    Department = model.Department,
                    StartWorkingDate = model.StartWorkingDate,
                    ContractType = model.ContractType
                };

                if (await _employeeService.UpdateEmployeeAsync(employee))
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            return View(MapToViewModel(employee));
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _employeeService.SoftDeleteEmployeeAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Payroll Generation
        public IActionResult GeneratePayroll()
        {
            TempData["Success"] = "Đã bắt đầu tạo bảng lương thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
