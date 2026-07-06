using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManager.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductUnitId",
                schema: "Product",
                table: "ProductPricingRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductUnitId",
                schema: "Product",
                table: "ProductLicenseOfferings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductUnits",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductUnits_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Product",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductUnits_UnitDefinitions_UnitDefinitionId",
                        column: x => x.UnitDefinitionId,
                        principalSchema: "Product",
                        principalTable: "UnitDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPricingRules_ProductUnitId",
                schema: "Product",
                table: "ProductPricingRules",
                column: "ProductUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLicenseOfferings_ProductUnitId",
                schema: "Product",
                table: "ProductLicenseOfferings",
                column: "ProductUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_Product_UnitDefinition",
                schema: "Product",
                table: "ProductUnits",
                columns: new[] { "ProductId", "UnitDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_ProductId_Code",
                schema: "Product",
                table: "ProductUnits",
                columns: new[] { "ProductId", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_UnitDefinitionId",
                schema: "Product",
                table: "ProductUnits",
                column: "UnitDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductLicenseOfferings_ProductUnits_ProductUnitId",
                schema: "Product",
                table: "ProductLicenseOfferings",
                column: "ProductUnitId",
                principalSchema: "Product",
                principalTable: "ProductUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPricingRules_ProductUnits_ProductUnitId",
                schema: "Product",
                table: "ProductPricingRules",
                column: "ProductUnitId",
                principalSchema: "Product",
                principalTable: "ProductUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductLicenseOfferings_ProductUnits_ProductUnitId",
                schema: "Product",
                table: "ProductLicenseOfferings");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPricingRules_ProductUnits_ProductUnitId",
                schema: "Product",
                table: "ProductPricingRules");

            migrationBuilder.DropTable(
                name: "ProductUnits",
                schema: "Product");

            migrationBuilder.DropIndex(
                name: "IX_ProductPricingRules_ProductUnitId",
                schema: "Product",
                table: "ProductPricingRules");

            migrationBuilder.DropIndex(
                name: "IX_ProductLicenseOfferings_ProductUnitId",
                schema: "Product",
                table: "ProductLicenseOfferings");

            migrationBuilder.DropColumn(
                name: "ProductUnitId",
                schema: "Product",
                table: "ProductPricingRules");

            migrationBuilder.DropColumn(
                name: "ProductUnitId",
                schema: "Product",
                table: "ProductLicenseOfferings");
        }
    }
}
