using SalesManagement.DAL.Entities;

namespace SalesManagement.BLL.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<Employee>> GetAllEmployeesAsync(string? searchString);
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<bool> CreateEmployeeWithUserAsync(Employee employee, User user);
        Task<bool> UpdateEmployeeAsync(Employee employee);
        Task<bool> SoftDeleteEmployeeAsync(int id);
    }
}
