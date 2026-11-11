using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;

        public EmployeeService(IEmployeeRepository employeeRepository, IUserRepository userRepository)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(string? searchString)
        {
            var employees = await _employeeRepository.GetAllAsync();
            employees = employees.Where(e => !e.IsDeleted);

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                employees = employees.Where(e =>
                    e.User != null && e.User.FullName != null && e.User.FullName.ToLower().Contains(searchString) ||
                    e.Position != null && e.Position.ToLower().Contains(searchString));
            }
            
            return employees;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _employeeRepository.GetByIdAsync(id);
        }

        public async Task<bool> CreateEmployeeWithUserAsync(Employee employee, User user)
        {
            // Logic handled by the repository/context usually, but we need atomicity
            // Check if user already exists
            if (await _userRepository.GetByEmailAsync(user.Email) != null || await _userRepository.GetByUsernameAsync(user.Username) != null)
            {
                return false;
            }

            try 
            {
                // Note: Standard 3-Layer would prefer the Repository to handle transactions or use a UnitOfWork. 
                // For now we assume the repositories share the same DbContext (Scoped).
                
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash ?? "123456");
                user.CreatedDate = DateTime.Now;
                user.UpdatedDate = DateTime.Now;
                user.IsActive = true;
                user.IsDeleted = false;

                await _userRepository.AddAsync(user);
                await _userRepository.SaveAsync();

                employee.UserId = user.UserId;
                employee.IsDeleted = false;
                await _employeeRepository.AddAsync(employee);
                await _employeeRepository.SaveAsync();

                return true;
            }
            catch (Exception)
            {
                // In a professional setup, we would rollback here if using UnitOfWork
                return false;
            }
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            var existing = await _employeeRepository.GetByIdAsync(employee.EmployeeId);
            if (existing == null) return false;

            existing.Position = employee.Position;
            existing.BasicSalary = employee.BasicSalary;
            existing.StartWorkingDate = employee.StartWorkingDate;
            existing.Department = employee.Department;
            existing.ContractType = employee.ContractType;

            // If user info is included in the parameter, update it too
            if (employee.User != null && existing.User != null)
            {
                existing.User.FullName = employee.User.FullName;
                existing.User.PhoneNumber = employee.User.PhoneNumber;
                existing.User.Role = employee.User.Role;
                existing.User.Email = employee.User.Email;
                existing.User.Username = employee.User.Email; // Keep username same as email for simplicity
                existing.User.UpdatedDate = DateTime.Now;
            }

            _employeeRepository.Update(existing);
            await _employeeRepository.SaveAsync();
            return true;
        }

        public async Task<bool> SoftDeleteEmployeeAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null) return false;

            employee.IsDeleted = true;
            if (employee.User != null)
            {
                employee.User.IsActive = false;
                employee.User.UpdatedDate = DateTime.Now;
            }

            _employeeRepository.Update(employee);
            await _employeeRepository.SaveAsync();
            return true;
        }
    }
}
