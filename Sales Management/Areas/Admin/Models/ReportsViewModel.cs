using System;
using System.Collections.Generic;

namespace SalesManagement.Web.Areas.Admin.Models
{
    // ─── Revenue Report ────────────────────────────────────────────────────────
    public class RevenueReportViewModel
    {
        // KPIs
        public decimal TotalRevenue      { get; set; }
        public int     TotalOrders       { get; set; }
        public decimal AverageOrderValue { get; set; }
        public double  CompletionRate    { get; set; }
        public double  CancellationRate  { get; set; }

        // Period comparison
        public decimal RevenueGrowth { get; set; }
        public decimal OrdersGrowth  { get; set; }

        // Filter context
        public string   Timeframe { get; set; } = "month";
        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }

        // Chart data (pre-built by service)
        public List<string>  ChartLabels      { get; set; } = new();
        public List<decimal> RevenueChartData { get; set; } = new();
        public List<int>     OrdersChartData  { get; set; } = new();
    }

    // ─── Product Report ────────────────────────────────────────────────────────
    public class ProductReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate   { get; set; }

        public List<TopProductViewModel>      TopSellingProducts { get; set; } = new();
        public List<ProductInventoryViewModel> LowStockProducts  { get; set; } = new();

        // Not populated from DB currently — kept for future use
        public List<ProductInventoryViewModel> LongInStockProducts { get; set; } = new();

        // Inventory summary
        public decimal InventoryTurnoverRate { get; set; }
        public int     TotalItemsInStock     { get; set; }
        public decimal TotalInventoryValue   { get; set; }
    }

    // ─── Financial Report ──────────────────────────────────────────────────────
    public class FinancialReportViewModel
    {
        public DateTime StartDate    { get; set; }
        public DateTime EndDate      { get; set; }

        public decimal NetRevenue   { get; set; }

        public List<InvoiceStatViewModel> InvoiceStats { get; set; } = new();

    }

    // ─── Shared sub-view models ────────────────────────────────────────────────
    public class TopProductViewModel
    {
        public string  ProductName      { get; set; } = string.Empty;
        public string  ProductCode      { get; set; } = string.Empty;
        public int     UnitsSold        { get; set; }
        public decimal RevenueGenerated { get; set; }
    }

    public class ProductInventoryViewModel
    {
        public int     ProductId     { get; set; }
        public string  Name          { get; set; } = string.Empty;
        public string  Code          { get; set; } = string.Empty;
        public int     StockQuantity { get; set; }
        public int     DaysInStock   { get; set; }
        public decimal Value         { get; set; }
    }

    public class InvoiceStatViewModel
    {
        public string  Status      { get; set; } = string.Empty;
        public int     Count       { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
