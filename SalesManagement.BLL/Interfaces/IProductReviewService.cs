using System.Collections.Generic;
using System.Threading.Tasks;
using SalesManagement.BLL.DTOs;

namespace SalesManagement.BLL.Interfaces;

public interface IProductReviewService
{
    Task<IEnumerable<ProductReviewDto>> GetProductReviewsAsync(int productId);
    Task<bool> AddReviewAsync(int userId, int productId, int rating, string comment);
    Task<bool> HasPurchasedProductAsync(int userId, int productId);
    Task<double> GetAverageRatingAsync(int productId);
}
