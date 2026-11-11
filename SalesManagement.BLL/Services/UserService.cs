using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesManagement.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace SalesManagement.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly AppDbContext _context; // For transactions if needed, though UnitOfWork preferred

        public UserService(IUserRepository userRepository, IRepository<Employee> employeeRepository, AppDbContext context)
        {
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAdminUsersAsync(string? search, string? role, string? sortOrder)
        {
            var users = await _userRepository.GetAllAsync();
            users = users.Where(u => !u.IsDeleted);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                users = users.Where(u => u.Username.ToLower().Contains(search) || 
                                       u.Email.ToLower().Contains(search) || 
                                       (u.FullName != null && u.FullName.ToLower().Contains(search)));
            }

            if (!string.IsNullOrEmpty(role))
            {
                users = users.Where(u => u.Role == role);
            }

            users = sortOrder switch
            {
                "name_desc" => users.OrderByDescending(u => u.FullName ?? u.Username),
                "Date" => users.OrderBy(u => u.CreatedDate),
                "date_desc" => users.OrderByDescending(u => u.CreatedDate),
                _ => users.OrderBy(u => u.FullName ?? u.Username)
            };

            return users;
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            if (await _userRepository.GetByEmailAsync(user.Email) != null) return false;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    user.Username = user.Email;
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                    user.CreatedDate = DateTime.Now;
                    user.UpdatedDate = DateTime.Now;
                    user.IsDeleted = false;
                    user.IsActive = true;

                    await _userRepository.AddAsync(user);
                    await _userRepository.SaveAsync();

                    if (user.Role == "Sales")
                    {
                        var employee = new Employee { UserId = user.UserId, IsDeleted = false };
                        await _employeeRepository.AddAsync(employee);
                        await _employeeRepository.SaveAsync();
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }

        public async Task<bool> UpdateUserAsync(User user, string? newPassword)
        {
            var existing = await _userRepository.GetByIdAsync(user.UserId);
            if (existing == null) return false;

            existing.FullName = user.FullName;
            existing.Email = user.Email;
            existing.Username = user.Email; // Syncing username
            existing.PhoneNumber = user.PhoneNumber;
            existing.Role = user.Role;
            existing.IsActive = user.IsActive;
            existing.UpdatedDate = DateTime.Now;

            if (!string.IsNullOrEmpty(newPassword))
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            _userRepository.Update(existing);
            await _userRepository.SaveAsync();
            return true;
        }

        public async Task<bool> SoftDeleteUserAsync(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var user = await _userRepository.GetByIdAsync(id);
                    if (user == null) return false;

                    user.IsDeleted = true;
                    user.IsActive = false;
                    user.UpdatedDate = DateTime.Now;
                    
                    _userRepository.Update(user);

                    if (user.Role == "Sales")
                    {
                        var employees = await _employeeRepository.FindAsync(e => e.UserId == id);
                        var employee = employees.FirstOrDefault();
                        if (employee != null)
                        {
                            employee.IsDeleted = true;
                            _employeeRepository.Update(employee);
                        }
                    }

                    await _userRepository.SaveAsync();
                    await _employeeRepository.SaveAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }
    }
}
