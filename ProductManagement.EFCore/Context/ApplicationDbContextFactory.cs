using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace ProductManagement.EfCore.Context
{
    public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("PRODUCT_MANAGER_CONNECTION_STRING")
                ?? TryReadWebUiConnectionString()
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=ProductManagement;Trusted_Connection=True;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private static string? TryReadWebUiConnectionString()
        {
            var current = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (current is not null)
            {
                var appSettingsPath = Path.Combine(current.FullName, "ProductManagement.WebUI", "appsettings.json");
                if (File.Exists(appSettingsPath))
                {
                    using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));
                    if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
                        && connectionStrings.TryGetProperty("Default", out var defaultConnection)
                        && defaultConnection.ValueKind == JsonValueKind.String)
                    {
                        return defaultConnection.GetString();
                    }
                }

                current = current.Parent;
            }

            return null;
        }
    }
}
