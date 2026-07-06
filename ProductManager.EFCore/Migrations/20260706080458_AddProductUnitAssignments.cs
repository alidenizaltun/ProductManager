using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManager.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductUnitAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductLicenseOfferingUnits",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductLicenseOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductLicenseOfferingUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductLicenseOfferingUnits_ProductLicenseOfferings_ProductLicenseOfferingId",
                        column: x => x.ProductLicenseOfferingId,
                        principalSchema: "Product",
                        principalTable: "ProductLicenseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductLicenseOfferingUnits_ProductUnits_ProductUnitId",
                        column: x => x.ProductUnitId,
                        principalSchema: "Product",
                        principalTable: "ProductUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductPricingRuleUnits",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductPricingRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPricingRuleUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPricingRuleUnits_ProductPricingRules_ProductPricingRuleId",
                        column: x => x.ProductPricingRuleId,
                        principalSchema: "Product",
                        principalTable: "ProductPricingRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductPricingRuleUnits_ProductUnits_ProductUnitId",
                        column: x => x.ProductUnitId,
                        principalSchema: "Product",
                        principalTable: "ProductUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"
INSERT INTO [Product].[ProductLicenseOfferingUnits]
    ([Id], [ProductLicenseOfferingId], [ProductUnitId], [CreatedAt], [IsDeleted])
SELECT NEWID(), o.[Id], o.[ProductUnitId], SYSUTCDATETIME(), 0
FROM [Product].[ProductLicenseOfferings] o
WHERE o.[ProductUnitId] IS NOT NULL
  AND o.[IsDeleted] = 0;");

            migrationBuilder.Sql(@"
INSERT INTO [Product].[ProductPricingRuleUnits]
    ([Id], [ProductPricingRuleId], [ProductUnitId], [CreatedAt], [IsDeleted])
SELECT NEWID(), r.[Id], r.[ProductUnitId], SYSUTCDATETIME(), 0
FROM [Product].[ProductPricingRules] r
WHERE r.[ProductUnitId] IS NOT NULL
  AND r.[IsDeleted] = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLicenseOfferingUnits_Offering_Unit",
                schema: "Product",
                table: "ProductLicenseOfferingUnits",
                columns: new[] { "ProductLicenseOfferingId", "ProductUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductLicenseOfferingUnits_ProductUnitId",
                schema: "Product",
                table: "ProductLicenseOfferingUnits",
                column: "ProductUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPricingRuleUnits_ProductUnitId",
                schema: "Product",
                table: "ProductPricingRuleUnits",
                column: "ProductUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPricingRuleUnits_Rule_Unit",
                schema: "Product",
                table: "ProductPricingRuleUnits",
                columns: new[] { "ProductPricingRuleId", "ProductUnitId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductLicenseOfferingUnits",
                schema: "Product");

            migrationBuilder.DropTable(
                name: "ProductPricingRuleUnits",
                schema: "Product");
        }
    }
}
