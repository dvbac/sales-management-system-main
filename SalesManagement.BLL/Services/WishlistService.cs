using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SalesManagement.BLL.DTOs;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;

namespace SalesManagement.BLL.Services;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;

    public WishlistService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<WishlistDto>> GetUserWishlistAsync(int userId)
    {
        return await _context.Wishlists
            .Where(w => w.UserId == userId)
            .Select(w => new WishlistDto
            {
                WishlistId = w.WishlistId,
                ProductId = w.ProductId,
                ProductName = w.Product.Name,
                Price = w.Product.SellingPrice,
                ImageUrl = w.Product.ProductImages.Where(pi => pi.IsPrimary == true).Select(pi => pi.ImageUrl).FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<bool> AddToWishlistAsync(int userId, int productId)
    {
        if (await IsInWishlistAsync(userId, productId))
            return false;

        var wishlist = new Wishlist
        {
            UserId = userId,
            ProductId = productId
        };

        _context.Wishlists.Add(wishlist);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveFromWishlistAsync(int userId, int productId)
    {
        var item = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

        if (item == null)
            return false;

        _context.Wishlists.Remove(item);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> IsInWishlistAsync(int userId, int productId)
    {
        return await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
    }
}
