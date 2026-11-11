using System.Threading.Tasks;
using SalesManagement.DAL.Entities;

namespace SalesManagement.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<User?> ValidateUser(string username, string password);
        Task<User?> RegisterUser(string username, string email, string password, string? fullName, string? phoneNumber);
        Task CheckInSalesEmployee(int userId);
        Task CheckOutSalesEmployee(int userId, string reason);
        Task<User?> GetUserProfileAsync(int userId);
        Task<bool> UpdateUserProfileAsync(int userId, string fullName, string email, string phone, string address, string? avatarUrl);
    }
}
