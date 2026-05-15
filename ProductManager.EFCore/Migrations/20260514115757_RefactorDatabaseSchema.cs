using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManager.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDatabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SoftwarePricingTiers_ProductId_Model_Unit_Min",
                schema: "Product",
                table: "SoftwarePricingTiers");

            migrationBuilder.DropColumn(
                name: "LicenseModel",
                schema: "Product",
                table: "SoftwarePricingTiers");

            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "Product",
                table: "SoftwarePricingTiers");

            migrationBuilder.DropColumn(
                name: "LicenseModel",
                schema: "Product",
                table: "ProductSoftwareProfiles");

            migrationBuilder.DropColumn(
                name: "SeatCount",
                schema: "Product",
                table: "ProductSoftwareProfiles");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                schema: "Product",
                table: "Products");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductLicenseOfferingId",
                schema: "Product",
                table: "SoftwarePricingTiers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UnitDefinitionId",
                schema: "Product",
                table: "SoftwarePricingTiers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UnitDefinitionId",
                schema: "Product",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UnitDefinitions",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitDefinitions", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitDefinitionId",
                schema: "Product",
                table: "Products",
                column: "UnitDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitDefinitions_Code",
                schema: "Product",
                table: "UnitDefinitions",
                column: "Code",
                unique: true);

            // Mevcut SoftwarePricingTiers satırları varsayılan GUID ile eklenir; FK oluşturulmadan önce geçerli anahtarlara bağlanmalı.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Product].[UnitDefinitions] WHERE [Code] = N'MIGRATE_DEFAULT')
BEGIN
    INSERT INTO [Product].[UnitDefinitions] ([Id],[Code],[Name],[Description],[IsActive],[SortOrder],[CreatedAt],[IsDeleted])
    VALUES (NEWID(), N'MIGRATE_DEFAULT', N'Migration default unit', NULL, 1, 0, SYSUTCDATETIME(), 0);
END;

DECLARE @DefaultUnit uniqueidentifier = (SELECT TOP 1 [Id] FROM [Product].[UnitDefinitions] ORDER BY [SortOrder], [Name]);

UPDATE [Product].[SoftwarePricingTiers]
SET [UnitDefinitionId] = @DefaultUnit
WHERE [UnitDefinitionId] = '00000000-0000-0000-0000-000000000000';

UPDATE t
SET [ProductLicenseOfferingId] = o.[Id]
FROM [Product].[SoftwarePricingTiers] t
CROSS APPLY (
    SELECT TOP 1 p.[Id]
    FROM [Product].[ProductLicenseOfferings] p
    WHERE p.[ProductId] = t.[ProductId] AND p.[IsDeleted] = 0
    ORDER BY p.[SortOrder], p.[Name]
) o
WHERE t.[ProductLicenseOfferingId] = '00000000-0000-0000-0000-000000000000';

DELETE FROM [Product].[SoftwarePricingTiers]
WHERE [ProductLicenseOfferingId] = '00000000-0000-0000-0000-000000000000'
   OR [UnitDefinitionId] = '00000000-0000-0000-0000-000000000000';
");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_UnitDefinitions_UnitDefinitionId",
                schema: "Product",
                table: "Products",
                column: "UnitDefinitionId",
                principalSchema: "Product",
                principalTable: "UnitDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwarePricingTiers_ProductLicenseOfferings_ProductLicenseOfferingId",
                schema: "Product",
                table: "SoftwarePricingTiers",
                column: "ProductLicenseOfferingId",
                principalSchema: "Product",
                principalTable: "ProductLicenseOfferings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwarePricingTiers_UnitDefinitions_UnitDefinitionId",
                schema: "Product",
                table: "SoftwarePricingTiers",
                column: "UnitDefinitionId",
                principalSchema: "Product",
                principalTable: "UnitDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_UnitDefinitions_UnitDefinitionId",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwarePricingTiers_ProductLicenseOfferings_ProductLicenseOfferingId",
                schema: "Product",
                table: "SoftwarePricingTiers");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwarePricingTiers_UnitDefinitions_UnitDefinitionId",
                schema: "Product",
                table: "SoftwarePricingTiers");

            migrationBuilder.DropTable(
                name: "UnitDefinitions",
                schema: "Product");

            migrationBuilder.DropIndex(
                name: "IX_SoftwarePricingTiers_Product_Offering_Unit_Min",
                schema: "Product",
                table: "SoftwarePricingTiers");

            migrationBuilder.DropIndex(
                name: "IX_SoftwarePricingTiers_ProductLicenseOfferingId",
                schema: "Product",
                table: "SoftwarePricingTiers");

            migrationBuilder.DropIndex(
                name: "IX_SoftwarePricingTiers_UnitDefinitionId",
                schema: "Product",
                table: "SoftwarePricingTiers");

            migrationBuilder.DropIndex(
                name: "IX_Products_UnitDefinitionId",
                schema: "Product",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductLicenseOfferingId",
                schema: "Product",
                table: "SoftwarePricingTiers");

            migrationBuilder.DropColumn(
                name: "UnitDefinitionId",
                schema: "Product",
                table: "SoftwarePricingTiers");

            migrationBuilder.DropColumn(
                name: "UnitDefinitionId",
                schema: "Product",
                table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "LicenseModel",
                schema: "Product",
                table: "SoftwarePricingTiers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                schema: "Product",
                table: "SoftwarePricingTiers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LicenseModel",
                schema: "Product",
                table: "ProductSoftwareProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeatCount",
                schema: "Product",
                table: "ProductSoftwareProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                schema: "Product",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SoftwarePricingTiers_ProductId_Model_Unit_Min",
                schema: "Product",
                table: "SoftwarePricingTiers",
                columns: new[] { "ProductId", "LicenseModel", "Unit", "MinUnits" });
        }
    }
}
