using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductManager.EFCore.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemManagementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "System");

            migrationBuilder.CreateTable(
                name: "Integrations",
                schema: "System",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CredentialsProtected = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSystemManaged = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastTestedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastTestSucceeded = table.Column<bool>(type: "bit", nullable: true),
                    LastTestMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Integrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                schema: "System",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEditable = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Integrations_ProviderKey",
                schema: "System",
                table: "Integrations",
                column: "ProviderKey",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Category_Key",
                schema: "System",
                table: "SystemSettings",
                columns: new[] { "Category", "Key" },
                unique: true);

            migrationBuilder.InsertData(
                schema: "System",
                table: "SystemSettings",
                columns: new[] { "Id", "Category", "Key", "Value", "DataType", "DisplayName", "Description", "IsEditable", "SortOrder", "UpdatedBy", "CreatedAt", "UpdatedAt", "DeletedAt", "IsDeleted" },
                values: new object[,]
                {
                    { new Guid("8f1a1f0a-0001-4a1e-9c1a-000000000001"), "Genel", "CompanyName", "Deva Yazılım", "String", "Şirket Adı", "Uygulama genelinde görünen şirket adı.", true, 0, null, DateTime.Now, null, null, false },
                    { new Guid("8f1a1f0a-0001-4a1e-9c1a-000000000002"), "Genel", "DefaultCurrency", "TRY", "String", "Varsayılan Para Birimi", "Fiyatlandırma ekranlarında kullanılacak varsayılan para birimi kodu.", true, 1, null, DateTime.Now, null, null, false },
                    { new Guid("8f1a1f0a-0001-4a1e-9c1a-000000000003"), "Genel", "DateFormat", "dd.MM.yyyy", "String", "Tarih Formatı", "Arayüzde tarihlerin görüntülenme biçimi.", true, 2, null, DateTime.Now, null, null, false },
                    { new Guid("8f1a1f0a-0001-4a1e-9c1a-000000000004"), "Genel", "TimeZone", "Europe/Istanbul", "String", "Saat Dilimi", "Uygulamanın varsayılan saat dilimi.", true, 3, null, DateTime.Now, null, null, false },
                    { new Guid("8f1a1f0a-0001-4a1e-9c1a-000000000005"), "Genel", "MaxUploadSizeMb", "10", "Number", "Maksimum Dosya Yükleme Boyutu (MB)", "Ürün medyası gibi dosya yüklemelerinde izin verilen maksimum boyut.", true, 4, null, DateTime.Now, null, null, false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Integrations",
                schema: "System");

            migrationBuilder.DropTable(
                name: "SystemSettings",
                schema: "System");
        }
    }
}
