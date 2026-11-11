using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SalesManagement.BLL.DTOs;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;

namespace SalesManagement.BLL.Services;

public class ProductReviewService : IProductReviewService
{
    private readonly AppDbContext _context;

    public ProductReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductReviewDto>> GetProductReviewsAsync(int productId)
    {
        return await _context.ProductReviews
            .Where(r => r.ProductId == productId)
            .Select(r => new ProductReviewDto
            {
                ReviewId = r.ReviewId,
                ProductId = r.ProductId,
                UserId = r.UserId,
                FullName = r.User.FullName,
                Avatar = r.User.Avatar,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedDate = r.CreatedDate,
                IsVerifiedPurchase = r.IsVerifiedPurchase
            })
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> AddReviewAsync(int userId, int productId, int rating, string comment)
    {
        var isPurchased = await HasPurchasedProductAsync(userId, productId);
        
        var review = new ProductReview
        {
            UserId = userId,
            ProductId = productId,
            Rating = rating,
            Comment = comment,
            IsVerifiedPurchase = isPurchased
        };

        _context.ProductReviews.Add(review);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> HasPurchasedProductAsync(int userId, int productId)
    {
        return await _context.Orders
            .AnyAsync(o => o.CustomerId == _context.Customers.Where(c => c.UserId == userId).Select(c => c.CustomerId).FirstOrDefault()
                        && o.Status == "Completed" 
                        && o.OrderDetails.Any(od => od.ProductId == productId));
    }

    public async Task<double> GetAverageRatingAsync(int productId)
    {
        var reviews = _context.ProductReviews.Where(r => r.ProductId == productId);
        if (!await reviews.AnyAsync()) return 0;
        return await reviews.AverageAsync(r => r.Rating);
    }
}
