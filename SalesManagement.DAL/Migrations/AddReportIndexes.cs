using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddReportIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Index for Orders.OrderDate — used in all date-range report queries
            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            // Index for Orders.Status — used in revenue/financial filters
            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            // Index for Products.Status — used in inventory queries
            migrationBuilder.CreateIndex(
                name: "IX_Products_Status",
                table: "Products",
                column: "Status");

            // Index for Products.CreatedDate
            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedDate",
                table: "Products",
                column: "CreatedDate");

            // Composite index for WalletTransactions (CreatedDate, Status)
            // Used in Financial report date-range + status filter
            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CreatedDate_Status",
                table: "WalletTransactions",
                columns: new[] { "CreatedDate", "Status" });

            // Composite index for OrderDetails (OrderId, ProductId)
            // Used in top-selling product GROUP BY query
            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId_ProductId",
                table: "OrderDetails",
                columns: new[] { "OrderId", "ProductId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Orders_OrderDate",                       table: "Orders");
            migrationBuilder.DropIndex(name: "IX_Orders_Status",                          table: "Orders");
            migrationBuilder.DropIndex(name: "IX_Products_Status",                        table: "Products");
            migrationBuilder.DropIndex(name: "IX_Products_CreatedDate",                   table: "Products");
            migrationBuilder.DropIndex(name: "IX_WalletTransactions_CreatedDate_Status",  table: "WalletTransactions");
            migrationBuilder.DropIndex(name: "IX_OrderDetails_OrderId_ProductId",         table: "OrderDetails");
        }
    }
}
