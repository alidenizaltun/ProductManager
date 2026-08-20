using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProductManagement.EfCore.Context;

#nullable disable

namespace ProductManagement.EFCore.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260623090000_AddProductPricingRules")]
    public partial class AddProductPricingRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductPricingRules",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceAdjustmentJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConditionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SalesChannel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CustomerGroupCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductLicenseOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPricingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPricingRules_ProductLicenseOfferings_ProductLicenseOfferingId",
                        column: x => x.ProductLicenseOfferingId,
                        principalSchema: "Product",
                        principalTable: "ProductLicenseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductPricingRules_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalSchema: "Product",
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductPricingRules_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Product",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPricingRules_Product_Active_Priority",
                schema: "Product",
                table: "ProductPricingRules",
                columns: new[] { "ProductId", "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPricingRules_ProductId_Code",
                schema: "Product",
                table: "ProductPricingRules",
                columns: new[] { "ProductId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPricingRules_ProductLicenseOfferingId",
                schema: "Product",
                table: "ProductPricingRules",
                column: "ProductLicenseOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPricingRules_ProductVariantId",
                schema: "Product",
                table: "ProductPricingRules",
                column: "ProductVariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductPricingRules",
                schema: "Product");
        }
    }
}
