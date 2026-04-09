using Microsoft.Extensions.Configuration;

namespace ProductManager.Presentation.Infrastructure.Helpers
{
    public class ConfigurationHelper
    {
        private readonly IConfiguration _configuration;

        public ConfigurationHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetConfigValue(string key)
        {
            try
            {
                return _configuration.GetSection(key).Value ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
