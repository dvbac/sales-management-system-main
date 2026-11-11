using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.BLL.Interfaces;

namespace SalesManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PayrollController : Controller
    {
        private readonly IPayrollService _payrollService;

        public PayrollController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        public async Task<IActionResult> Index(int? month, int? year)
        {
            var m = month ?? DateTime.Now.Month;
            var y = year ?? DateTime.Now.Year;

            var payrolls = await _payrollService.GetPayrollsAsync(m, y);
            ViewBag.Month = m;
            ViewBag.Year = y;
            return View(payrolls);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(int month, int year)
        {
            await _payrollService.GeneratePayrollForAllAsync(month, year);
            return RedirectToAction(nameof(Index), new { month, year });
        }

        public async Task<IActionResult> Details(int id)
        {
            var payroll = await _payrollService.GetPayrollByIdAsync(id);
            if (payroll == null) return NotFound();
            return View(payroll);
        }
    }
}
