using Microsoft.EntityFrameworkCore;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;

namespace SalesManagement.Web.Extensions
{
    /// <summary>
    /// Ensures the default admin account (from "DefaultAdmin" in appsettings.json) exists
    /// and its credentials always match the configured values.
    /// </summary>
    public static class AdminSeeder
    {
        public static async Task SeedDefaultAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Read credentials from appsettings.json or use default
            var email    = configuration["DefaultAdmin:Email"]    ?? "admin@salesmanager.com";
            var password = configuration["DefaultAdmin:Password"] ?? "123456";
            var username = configuration["DefaultAdmin:Username"] ?? "admin";
            var fullName = configuration["DefaultAdmin:FullName"] ?? "System Administrator";

            // Find existing account by Email OR Username (avoids duplicate-key on either column)
            var existingAdmin = await db.Users
                .FirstOrDefaultAsync(u => !u.IsDeleted && (u.Email == email || u.Username == username));

            if (existingAdmin != null)
            {
                // Giữ nguyên tài khoản admin, chỉ đảm bảo vai trò và trạng thái
                existingAdmin.Role = "Admin";
                existingAdmin.IsActive = true;
                await db.SaveChangesAsync();
                return;
            }

            // No matching account → create fresh
            var adminUser = new User
            {
                Username     = username,
                Email        = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName     = fullName,
                Role         = "Admin",
                IsActive     = true,
                IsDeleted    = false,
                CreatedDate  = DateTime.Now,
                UpdatedDate  = DateTime.Now
            };

            db.Users.Add(adminUser);
            await db.SaveChangesAsync();
        }
    }
}
