using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.DAL.Entities;
using SalesManagement.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SalesManagement.BLL.Interfaces;

namespace SalesManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IHubContext<SystemHub> _hubContext;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeService employeeService, IHubContext<SystemHub> hubContext, ILogger<EmployeesController> logger)
        {
            _employeeService = employeeService;
            _hubContext = hubContext;
            _logger = logger;
        }

        // GET: Admin/Employees (Danh sách nhân viên, hỗ trợ tìm kiếm và lọc)
        public async Task<IActionResult> Index(string searchString, string contractType)
        {
            var employees = await _employeeService.GetAllEmployeesAsync(searchString);

            if (!string.IsNullOrEmpty(contractType))
            {
                employees = employees.Where(e => e.ContractType == contractType);
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentContractType"] = contractType;

            return View(employees);
        }

        // GET: Admin/Employees/Details/5 (Xem chi tiết nhân viên)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // GET: Admin/Employees/Create (Form tạo nhân viên mới)
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Employees/Create (Xử lý tạo nhân viên)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Position,BasicSalary,StartWorkingDate,Department,ContractType")] Employee employee, string FullName, string Email, string Password, string Role)
        {
            ModelState.Remove("User");
            
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin tài khoản.");
                }
                else
                {
                    var user = new User
                    {
                        FullName = FullName,
                        Email = Email,
                        Username = Email, 
                        PasswordHash = Password, // Hashed inside service
                        Role = Role
                    };

                    var result = await _employeeService.CreateEmployeeWithUserAsync(employee, user);
                    if (result)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "ReloadData");
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Email/Username đã tồn tại hoặc lỗi hệ thống.");
                    }
                }
            }
            ViewBag.FullName = FullName;
            ViewBag.Email = Email;
            ViewBag.Role = Role;
            return View(employee);
        }

        // GET: Admin/Employees/Edit/5 (Form chỉnh sửa nhân viên)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();
            
            return View(employee);
        }

        // POST: Admin/Employees/Edit/5 (Cập nhật nhân viên)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EmployeeId,Position,BasicSalary,StartWorkingDate,Department,ContractType")] Employee employeeInput, string FullName, string Email, string Role, string PhoneNumber)
        {
            if (id != employeeInput.EmployeeId) return NotFound();
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    employeeInput.User = new User
                    {
                        FullName = FullName,
                        Email = Email,
                        Role = Role,
                        PhoneNumber = PhoneNumber
                    };

                    var result = await _employeeService.UpdateEmployeeAsync(employeeInput);
                    if (result)
                    {
                        await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "ReloadData");
                        return RedirectToAction(nameof(Index));
                    }
                    
                    ModelState.AddModelError("", "Lỗi khi cập nhật hoặc Email đã tồn tại.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating employee");
                    ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
                }
            }
            return View(employeeInput);
        }

        // GET: Admin/Employees/Delete/5 (Xác nhận xóa)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _employeeService.GetEmployeeByIdAsync(id.Value);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Admin/Employees/Delete/5 (Xóa mềm nhân viên)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _employeeService.SoftDeleteEmployeeAsync(id);
            if (result)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "UserDeleted");
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
    }
}
