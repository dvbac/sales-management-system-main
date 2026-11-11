using Microsoft.EntityFrameworkCore;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using SalesManagement.BLL.Interfaces;

namespace SalesManagement.BLL.Services
{
    public class PayrollService : IPayrollService
    {
        private readonly AppDbContext _context;
        private const int StandardWorkDays = 22;

        public PayrollService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payroll>> GetPayrollsAsync(int month, int year)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.User)
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();
        }

        public async Task<Payroll?> GetPayrollByIdAsync(int id)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .ThenInclude(e => e.User)
                .FirstOrDefaultAsync(p => p.PayrollId == id);
        }

        public async Task GeneratePayrollForAllAsync(int month, int year)
        {
            var employees = await _context.Employees
                .Where(e => !e.IsDeleted)
                .ToListAsync();

            foreach (var emp in employees)
            {
                var exists = await _context.Payrolls
                    .AnyAsync(p => p.EmployeeId == emp.EmployeeId && p.Month == month && p.Year == year);
                
                if (exists) continue;

                await CalculatePayrollForEmployeeAsync(emp.EmployeeId, month, year);
            }
        }

        public async Task<Payroll?> CalculatePayrollForEmployeeAsync(int employeeId, int month, int year)
        {
            var employee = await _context.Employees
                .Include(e => e.EmployeeSalaryComponents)
                .ThenInclude(esc => esc.SalaryComponent)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null) return null;

            decimal basicSalary = employee.BasicSalary ?? 0;
            decimal hourlyRate = basicSalary / (StandardWorkDays * 8);
            decimal grossEarnings = basicSalary;

            decimal totalAllowances = 0;
            decimal totalDeductions = 0;

            foreach (var comp in employee.EmployeeSalaryComponents)
            {
                var amount = comp.Amount;
                if (comp.SalaryComponent.IsPercentage) amount = basicSalary * (comp.Amount / 100); 

                if (comp.SalaryComponent.Type == "Allowance") totalAllowances += amount;
                else if (comp.SalaryComponent.Type == "Deduction") totalDeductions += amount;
            }

            decimal grossSalary = grossEarnings + totalAllowances - totalDeductions;
            decimal penalty = 0;
            decimal taxIncome = Math.Max(0, grossSalary - 11000000);
            decimal taxAmount = CalculateTax(taxIncome);
            decimal netSalary = grossSalary - taxAmount - penalty;

            var payroll = new Payroll
            {
                EmployeeId = employeeId,
                Month = month,
                Year = year,
                BaseSalary = basicSalary,
                TotalAllowances = totalAllowances, 
                TotalDeductions = totalDeductions,
                TaxAmount = taxAmount,
                Penalty = penalty,
                NetSalary = netSalary,
                TotalSalary = grossSalary,
                Status = "Pending",
                CreatedDate = DateTime.Now
            };

            _context.Payrolls.Add(payroll);
            await _context.SaveChangesAsync();
            return payroll;
        }

        private decimal CalculateTax(decimal income)
        {
            if (income <= 0) return 0;
            if (income <= 5000000) return income * 0.05m;
            if (income <= 10000000) return 5000000 * 0.05m + (income - 5000000) * 0.1m;
            if (income <= 18000000) return 5000000 * 0.05m + 5000000 * 0.1m + (income - 10000000) * 0.15m;
            return 5000000 * 0.05m + 5000000 * 0.1m + 8000000 * 0.15m + (income - 18000000) * 0.2m;
        }
    }
}
