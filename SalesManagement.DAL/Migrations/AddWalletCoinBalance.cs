using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletCoinBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thêm cột CoinBalance vào bảng Wallets (lưu xu tích lũy)
            migrationBuilder.AddColumn<decimal>(
                name: "CoinBalance",
                table: "Wallets",
                type: "decimal(15,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoinBalance",
                table: "Wallets");
        }
    }
}
