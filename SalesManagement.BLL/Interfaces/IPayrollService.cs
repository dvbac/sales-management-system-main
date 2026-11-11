using System.Threading.Tasks;
using SalesManagement.DAL.Entities;
using System.Collections.Generic;

namespace SalesManagement.BLL.Interfaces
{
    public interface IPayrollService
    {
        Task GeneratePayrollForAllAsync(int month, int year);
        Task<Payroll?> CalculatePayrollForEmployeeAsync(int employeeId, int month, int year);
        Task<IEnumerable<Payroll>> GetPayrollsAsync(int month, int year);
        Task<Payroll?> GetPayrollByIdAsync(int id);
    }
}
