using System;
using System.Threading.Tasks;
using SalesManagement.BLL.DTOs;

namespace SalesManagement.BLL.Interfaces
{
    public interface IReportService
    {
        Task<RevenueReportDTO> GetRevenueReportAsync(string timeframe, DateTime? startDate, DateTime? endDate);
        Task<ProductReportDTO> GetProductReportAsync(DateTime? startDate, DateTime? endDate);
        Task<FinancialReportDTO> GetFinancialReportAsync(DateTime? startDate, DateTime? endDate);
    }
}
