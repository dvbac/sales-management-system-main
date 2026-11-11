using Microsoft.EntityFrameworkCore;
using SalesManagement.BLL.Interfaces;
using SalesManagement.BLL.DTOs;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<List<CartItemDTO>> GetCartByUserIdAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return new List<CartItemDTO>();

            return cart.CartItems.Select(ci => new CartItemDTO
            {
                ProductId = ci.ProductId,
                Name = ci.Product.Name,
                ImageUrl = ci.Product.ProductImages?.FirstOrDefault(img => img.IsPrimary == true)?.ImageUrl 
                           ?? ci.Product.ProductImages?.FirstOrDefault()?.ImageUrl 
                           ?? "/images/no-image.png",
                Price = ci.Product.SellingPrice,
                Quantity = ci.Quantity
            }).ToList();
        }

        public async Task AddToCartAsync(int userId, int productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem { CartId = cart.CartId, ProductId = productId, Quantity = quantity });
            }

            cart.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return;

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                cart.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return;

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                cart.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MergeCartAsync(int userId, List<CartItemDTO> sessionCart)
        {
            if (sessionCart == null || !sessionCart.Any()) return;

            var cart = await GetOrCreateCartAsync(userId);

            foreach (var sessionItem in sessionCart)
            {
                var dbItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == sessionItem.ProductId);
                if (dbItem != null)
                {
                    dbItem.Quantity += sessionItem.Quantity;
                }
                else
                {
                    cart.CartItems.Add(new CartItem 
                    { 
                        CartId = cart.CartId,
                        ProductId = sessionItem.ProductId, 
                        Quantity = sessionItem.Quantity 
                    });
                }
            }

            cart.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartCountAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
        }

        public async Task<CartItemDTO?> GetCartItemAsync(int userId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            var item = cart?.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item == null) return null;

            return new CartItemDTO {
                ProductId = item.ProductId,
                Price = item.Product.SellingPrice,
                Quantity = item.Quantity
            };
        }
    }
}
