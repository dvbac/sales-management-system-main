using System.Collections.Generic;
using System.Threading.Tasks;
using SalesManagement.BLL.DTOs;

namespace SalesManagement.BLL.Interfaces;

public interface IWishlistService
{
    Task<IEnumerable<WishlistDto>> GetUserWishlistAsync(int userId);
    Task<bool> AddToWishlistAsync(int userId, int productId);
    Task<bool> RemoveFromWishlistAsync(int userId, int productId);
    Task<bool> IsInWishlistAsync(int userId, int productId);
}
