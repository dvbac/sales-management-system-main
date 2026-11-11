using SalesManagement.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Interfaces
{
    public interface IPromotionService
    {
        Task<IEnumerable<Promotion>> GetAllPromotionsAsync(string? statusFilter, string? searchString);
        Task<Promotion?> GetPromotionByIdAsync(int id);
        Task<bool> AddPromotionAsync(Promotion promotion);
        Task<bool> UpdatePromotionAsync(Promotion promotion);
        Task<bool> DeletePromotionAsync(int id);
        Task<(bool Success, string Message, decimal DiscountAmount)> ValidatePromotionAsync(string code, decimal orderValue);
    }
}
