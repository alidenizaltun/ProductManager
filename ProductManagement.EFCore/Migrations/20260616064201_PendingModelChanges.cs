using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalPrice",
                schema: "Product",
                table: "ProductModules");

            migrationBuilder.DropColumn(
                name: "CurrencyCode",
                schema: "Product",
                table: "ProductModules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalPrice",
                schema: "Product",
                table: "ProductModules",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CurrencyCode",
                schema: "Product",
                table: "ProductModules",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }
    }
}
