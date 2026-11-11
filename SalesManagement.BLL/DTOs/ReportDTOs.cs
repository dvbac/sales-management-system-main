using System;
using System.Collections.Generic;

namespace SalesManagement.BLL.DTOs
{
    // ─── Revenue Report ────────────────────────────────────────────────────────
    public class RevenueReportDTO
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // Aggregated stats – no raw entities sent over the wire
        public decimal TotalRevenue { get; set; }
        public decimal PrevRevenue { get; set; }
        public int CurrentOrdersCount { get; set; }
        public int PrevOrdersCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }

        // Pre-aggregated chart data (already grouped by period in the DB)
        public List<ChartPoint> ChartData { get; set; } = new();
    }

    public class ChartPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    // ─── Product Report ────────────────────────────────────────────────────────
    public class ProductReportDTO
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public List<TopProductDTO> TopSelling { get; set; } = new();
        public List<LowStockProductDTO> LowStock { get; set; } = new();

        // Summary counts (computed in DB)
        public int TotalItemsInStock { get; set; }
        public decimal TotalInventoryValue { get; set; }
    }

    public class TopProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal RevenueGenerated { get; set; }
    }

    public class LowStockProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal Value { get; set; }
    }

    // ─── Financial Report ──────────────────────────────────────────────────────
    public class FinancialReportDTO
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // Invoice stats – aggregated in DB
        public decimal PaidRevenue { get; set; }
        public int PaidCount { get; set; }
        public decimal PendingRevenue { get; set; }
        public int PendingCount { get; set; }
        public decimal RefundedRevenue { get; set; }
        public int RefundedCount { get; set; }

    }
}
