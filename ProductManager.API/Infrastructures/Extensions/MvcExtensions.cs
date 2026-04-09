using Microsoft.AspNetCore.Mvc;

namespace ProductManager.API.Infrastructures.Extensions
{
    public static class MvcExtensions
    {
        public static void ConfigureCacheProfiles(this MvcOptions options, IHostApplicationBuilder builder)
        {
            var cacheProfiles = builder.Configuration
                .GetSection("CacheProfiles")
                .GetChildren();

            foreach (var cacheProfile in cacheProfiles)
            {
                options.CacheProfiles
                    .Add(cacheProfile.Key,
                        value: cacheProfile.Get<CacheProfile>()!
                    );
            }
        }
    }
}
