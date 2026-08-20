using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.EFCore.Migrations
{
 /// <inheritdoc />
 public partial class AddProductUnitConversions : Migration
 {
 /// <inheritdoc />
 protected override void Up(MigrationBuilder migrationBuilder)
 {
 migrationBuilder.CreateTable(
 name: "ProductUnitConversions",
 schema: "Product",
 columns: table => new
 {
 Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
 ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
 FromUnitDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
 ToUnitDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
 ConversionFactor = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
 FromUnitRole = table.Column<int>(type: "int", nullable: false),
 IsActive = table.Column<bool>(type: "bit", nullable: false),
 CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
 UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
 DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
 IsDeleted = table.Column<bool>(type: "bit", nullable: false)
 },
 constraints: table =>
 {
 table.PrimaryKey("PK_ProductUnitConversions", x => x.Id);
 table.ForeignKey(
 name: "FK_ProductUnitConversions_Products_ProductId",
 column: x => x.ProductId,
 principalSchema: "Product",
 principalTable: "Products",
 principalColumn: "Id",
 onDelete: ReferentialAction.Cascade);
 table.ForeignKey(
 name: "FK_ProductUnitConversions_UnitDefinitions_FromUnitDefinitionId",
 column: x => x.FromUnitDefinitionId,
 principalSchema: "Product",
 principalTable: "UnitDefinitions",
 principalColumn: "Id",
  onDelete: ReferentialAction.Restrict);
 table.ForeignKey(
 name: "FK_ProductUnitConversions_UnitDefinitions_ToUnitDefinitionId",
 column: x => x.ToUnitDefinitionId,
 principalSchema: "Product",
 principalTable: "UnitDefinitions",
 principalColumn: "Id",
 onDelete: ReferentialAction.Restrict);
 });

 migrationBuilder.CreateIndex(
 name: "IX_ProductUnitConversions_FromUnitDefinitionId",
 schema: "Product",
 table: "ProductUnitConversions",
 column: "FromUnitDefinitionId");

 migrationBuilder.CreateIndex(
 name: "IX_ProductUnitConversions_Product_From_To",
 schema: "Product",
 table: "ProductUnitConversions",
 columns: new[] { "ProductId", "FromUnitDefinitionId", "ToUnitDefinitionId" },
 unique: true);

 migrationBuilder.CreateIndex(
 name: "IX_ProductUnitConversions_ToUnitDefinitionId",
 schema: "Product",
 table: "ProductUnitConversions",
 column: "ToUnitDefinitionId");
 }

 /// <inheritdoc />
 protected override void Down(MigrationBuilder migrationBuilder)
 {
 migrationBuilder.DropTable(
 name: "ProductUnitConversions",
 schema: "Product");
 }
 }
}
