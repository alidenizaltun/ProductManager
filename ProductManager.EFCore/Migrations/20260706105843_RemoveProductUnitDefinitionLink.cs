using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManager.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductUnitDefinitionLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_UnitDefinitions_UnitDefinitionId",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_UnitDefinitionId",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitDefinitionId",
                schema: "Product",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UnitDefinitionId",
                schema: "Product",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitDefinitionId",
                schema: "Product",
                table: "Products",
                column: "UnitDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_UnitDefinitions_UnitDefinitionId",
                schema: "Product",
                table: "Products",
                column: "UnitDefinitionId",
                principalSchema: "Product",
                principalTable: "UnitDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
