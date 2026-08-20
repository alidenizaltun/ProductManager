using Microsoft.Extensions.Configuration;

namespace ProductManagement.Shared.Infrastructure.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetActiveConnectionString(this IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:Default Value Was Not Found.");
            }

            return connectionString;
        }
    }
}
