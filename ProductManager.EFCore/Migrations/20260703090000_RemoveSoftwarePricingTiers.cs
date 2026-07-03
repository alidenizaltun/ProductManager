using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManager.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSoftwarePricingTiers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoftwarePricingTiers",
                schema: "Product");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoftwarePricingTiers",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductLicenseOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinUnits = table.Column<int>(type: "int", nullable: false),
                    MaxUnits = table.Column<int>(type: "int", nullable: true),
                    PricePerUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FlatFee = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwarePricingTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftwarePricingTiers_ProductLicenseOfferings_ProductLicenseOfferingId",
                        column: x => x.ProductLicenseOfferingId,
                        principalSchema: "Product",
                        principalTable: "ProductLicenseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoftwarePricingTiers_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Product",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SoftwarePricingTiers_UnitDefinitions_UnitDefinitionId",
                        column: x => x.UnitDefinitionId,
                        principalSchema: "Product",
                        principalTable: "UnitDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwarePricingTiers_Product_Offering_Unit_Min",
                schema: "Product",
                table: "SoftwarePricingTiers",
                columns: new[] { "ProductId", "ProductLicenseOfferingId", "UnitDefinitionId", "MinUnits" });

            migrationBuilder.CreateIndex(
                name: "IX_SoftwarePricingTiers_ProductLicenseOfferingId",
                schema: "Product",
                table: "SoftwarePricingTiers",
                column: "ProductLicenseOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftwarePricingTiers_UnitDefinitionId",
                schema: "Product",
                table: "SoftwarePricingTiers",
                column: "UnitDefinitionId");
        }
    }
}
