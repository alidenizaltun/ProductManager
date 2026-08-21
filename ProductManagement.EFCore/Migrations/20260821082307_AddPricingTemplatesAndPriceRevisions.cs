using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingTemplatesAndPriceRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceTemplateId",
                schema: "Product",
                table: "ProductPricingRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceTemplateVersion",
                schema: "Product",
                table: "ProductPricingRules",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PriceRevisions",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdjustmentType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RoundingMode = table.Column<int>(type: "int", nullable: false),
                    RoundingStep = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovalNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AppliedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RolledBackAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RolledBackByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceRevisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingTemplates",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateKind = table.Column<int>(type: "int", nullable: false),
                    UnitDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PricingTemplates_UnitDefinitions_UnitDefinitionId",
                        column: x => x.UnitDefinitionId,
                        principalSchema: "Product",
                        principalTable: "UnitDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceRevisionLines",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceRevisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetPath = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TargetLabel = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    OldValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    NewValue = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    IsExcluded = table.Column<bool>(type: "bit", nullable: false),
                    IsApplied = table.Column<bool>(type: "bit", nullable: false),
                    SkipReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceRevisionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceRevisionLines_PriceRevisions_PriceRevisionId",
                        column: x => x.PriceRevisionId,
                        principalSchema: "Product",
                        principalTable: "PriceRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceRevisionScopes",
                schema: "Product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceRevisionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScopeType = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetValue = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IsExclude = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceRevisionScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceRevisionScopes_PriceRevisions_PriceRevisionId",
                        column: x => x.PriceRevisionId,
                        principalSchema: "Product",
                        principalTable: "PriceRevisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductPricingRules_SourceTemplateId",
                schema: "Product",
                table: "ProductPricingRules",
                column: "SourceTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceRevisionLines_Revision_Product",
                schema: "Product",
                table: "PriceRevisionLines",
                columns: new[] { "PriceRevisionId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceRevisionLines_Revision_Target",
                schema: "Product",
                table: "PriceRevisionLines",
                columns: new[] { "PriceRevisionId", "TargetType", "TargetId", "TargetPath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceRevisions_Code",
                schema: "Product",
                table: "PriceRevisions",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PriceRevisions_Status",
                schema: "Product",
                table: "PriceRevisions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PriceRevisionScopes_Revision_Type",
                schema: "Product",
                table: "PriceRevisionScopes",
                columns: new[] { "PriceRevisionId", "ScopeType" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingTemplates_Code",
                schema: "Product",
                table: "PricingTemplates",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PricingTemplates_Kind_Active",
                schema: "Product",
                table: "PricingTemplates",
                columns: new[] { "TemplateKind", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingTemplates_UnitDefinitionId",
                schema: "Product",
                table: "PricingTemplates",
                column: "UnitDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductPricingRules_PricingTemplates_SourceTemplateId",
                schema: "Product",
                table: "ProductPricingRules",
                column: "SourceTemplateId",
                principalSchema: "Product",
                principalTable: "PricingTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductPricingRules_PricingTemplates_SourceTemplateId",
                schema: "Product",
                table: "ProductPricingRules");

            migrationBuilder.DropTable(
                name: "PriceRevisionLines",
                schema: "Product");

            migrationBuilder.DropTable(
                name: "PriceRevisionScopes",
                schema: "Product");

            migrationBuilder.DropTable(
                name: "PricingTemplates",
                schema: "Product");

            migrationBuilder.DropTable(
                name: "PriceRevisions",
                schema: "Product");

            migrationBuilder.DropIndex(
                name: "IX_ProductPricingRules_SourceTemplateId",
                schema: "Product",
                table: "ProductPricingRules");

            migrationBuilder.DropColumn(
                name: "SourceTemplateId",
                schema: "Product",
                table: "ProductPricingRules");

            migrationBuilder.DropColumn(
                name: "SourceTemplateVersion",
                schema: "Product",
                table: "ProductPricingRules");
        }
    }
}
