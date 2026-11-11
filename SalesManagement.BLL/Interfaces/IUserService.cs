using SalesManagement.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAdminUsersAsync(string? search, string? role, string? sortOrder);
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> CreateUserAsync(User user, string password);
        Task<bool> UpdateUserAsync(User user, string? newPassword);
        Task<bool> SoftDeleteUserAsync(int id);
    }
}
