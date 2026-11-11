using SalesManagement.DAL.Entities;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Interfaces;
using SalesManagement.DAL.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly AppDbContext _context;

        public AuthService(IUserRepository userRepository, ICustomerRepository customerRepository, AppDbContext context)
        {
            _userRepository = userRepository;
            _customerRepository = customerRepository;
            _context = context;
        }

        public async Task<User?> ValidateUser(string username, string password)
        {
            // Support login by username OR email
            var user = await _userRepository.GetByUsernameAsync(username)
                    ?? await _userRepository.GetByEmailAsync(username);

            if (user == null || !user.IsActive) return null;

            bool isValid = false;
            try
            {
                isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch (Exception)
            {
                if (user.PasswordHash == password)
                {
                    isValid = true;
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                }
            }

            if (!isValid) return null;

            user.LastLogin = DateTime.Now;
            _userRepository.Update(user);
            await _userRepository.SaveAsync();

            return user;
        }

        public async Task<User?> RegisterUser(string username, string email, string password, string? fullName, string? phoneNumber)
        {
            if (await _userRepository.GetByUsernameAsync(username) != null) return null;
            if (await _userRepository.GetByEmailAsync(email) != null) return null;

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Role = "Customer",
                IsActive = true,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveAsync();

            await _customerRepository.AddAsync(new Customer
            {
                UserId = user.UserId,
                FullName = user.FullName ?? username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedDate = DateTime.Now,
                Type = "Personal",
                CustomerLevel = "Regular"
            });
            await _customerRepository.SaveAsync();

            return user;
        }

        public async Task CheckInSalesEmployee(int userId)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employee == null) return;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var attendance = await _context.TimeAttendances.FirstOrDefaultAsync(a => a.EmployeeId == employee.EmployeeId && a.Date == today);

            if (attendance == null)
            {
                _context.TimeAttendances.Add(new TimeAttendance
                {
                    EmployeeId = employee.EmployeeId,
                    Date = today,
                    CheckInTime = DateTime.Now,
                    Status = "Present"
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task CheckOutSalesEmployee(int userId, string reason)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == userId);
            if (employee == null) return;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var attendance = await _context.TimeAttendances.FirstOrDefaultAsync(a => a.EmployeeId == employee.EmployeeId && a.Date == today);

            if (attendance != null && attendance.CheckOutTime == null)
            {
                attendance.CheckOutTime = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetUserProfileAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Customer)
                    .ThenInclude(c => c.Orders)
                        .ThenInclude(o => o.OrderDetails)
                            .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> UpdateUserProfileAsync(int userId, string fullName, string email, string phone, string address, string? avatarUrl)
        {
             var user = await _context.Users.FindAsync(userId);
             var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
             if (user == null || customer == null) return false;

             user.FullName = fullName;
             user.Email = email;
             user.PhoneNumber = phone;
             if (!string.IsNullOrEmpty(avatarUrl)) user.Avatar = avatarUrl;
             
             customer.FullName = fullName;
             customer.Email = email;
             customer.PhoneNumber = phone;
             customer.Address = address;

             _context.Users.Update(user);
             _context.Customers.Update(customer);
             await _context.SaveChangesAsync();
             return true;
        }
    }
}
