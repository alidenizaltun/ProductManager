using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class AddProductLicenseOffering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductLicenseOfferings",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LicenseModel = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    BillingPeriodUnit = table.Column<int>(type: "int", nullable: true),
                    BillingPeriodValue = table.Column<int>(type: "int", nullable: true),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false),
                    GracePeriodDays = table.Column<int>(type: "int", nullable: true),
                    TrialDays = table.Column<int>(type: "int", nullable: true),
                    ConvertToOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaxSeats = table.Column<int>(type: "int", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductLicenseOfferings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductLicenseOfferings_ProductLicenseOfferings_ConvertToOfferingId",
                        column: x => x.ConvertToOfferingId,
                        principalSchema: "Product",
                        principalTable: "ProductLicenseOfferings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductLicenseOfferings_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "Product",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductLicenseOfferings_ConvertToOfferingId",
                schema: "Product",
                table: "ProductLicenseOfferings",
                column: "ConvertToOfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLicenseOfferings_ProductId_Name",
                schema: "Product",
                table: "ProductLicenseOfferings",
                columns: new[] { "ProductId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductLicenseOfferings",
                schema: "Product");
        }
    }
}
