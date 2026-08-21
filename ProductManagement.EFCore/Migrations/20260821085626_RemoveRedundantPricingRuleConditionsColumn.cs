using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManagement.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantPricingRuleConditionsColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConditionsJson",
                schema: "Product",
                table: "ProductPricingRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConditionsJson",
                schema: "Product",
                table: "ProductPricingRules",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
