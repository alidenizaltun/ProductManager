using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class FixProductCodeFilteredUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCode",
                schema: "Product",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                schema: "Product",
                table: "Products",
                column: "ProductCode",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCode",
                schema: "Product",
                table: "Products");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                schema: "Product",
                table: "Products",
                column: "ProductCode",
                unique: true);
        }
    }
}
