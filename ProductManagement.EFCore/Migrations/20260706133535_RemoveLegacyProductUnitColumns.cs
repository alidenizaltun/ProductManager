using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyProductUnitColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductLicenseOfferings_ProductUnits_ProductUnitId",
                schema: "Product",
                table: "ProductLicenseOfferings");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductPricingRules_ProductUnits_ProductUnitId",
                schema: "Product",
                table: "ProductPricingRules");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
