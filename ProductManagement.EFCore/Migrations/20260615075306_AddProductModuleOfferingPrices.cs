using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductModuleOfferingPrices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductModuleOfferingPrices",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductLicenseOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductModuleOfferingPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductModuleOfferingPrices_ProductLicenseOfferings_ProductLicenseOfferingId",
                        column: x => x.ProductLicenseOfferingId,
                        principalSchema: "Product",
                        principalTable: "ProductLicenseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductModuleOfferingPrices_ProductModules_ProductModuleId",
                        column: x => x.ProductModuleId,
                        principalSchema: "Product",
                        principalTable: "ProductModules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductModuleOfferingPrices_Module_Offering",
                schema: "Product",
                table: "ProductModuleOfferingPrices",
                columns: new[] { "ProductModuleId", "ProductLicenseOfferingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductModuleOfferingPrices_ProductLicenseOfferingId",
                schema: "Product",
                table: "ProductModuleOfferingPrices",
                column: "ProductLicenseOfferingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductModuleOfferingPrices",
                schema: "Product");
        }
    }
}
