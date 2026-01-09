using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.WebPerformance.Options;

namespace Shared.WebPerformance.Builder
{
    public static class WebPerformanceExtensions
    {
        /// <summary>
        /// Adds Shared.WebPerformance infrastructure.
        /// Configures Native Output Caching with optional Redis persistence.
        /// </summary>
        public static IServiceCollection AddSharedWebPerformance(
            this IServiceCollection services,
            Action<WebPerformanceOptions> configure)
        {
            // 1. Configure Options
            var options = new WebPerformanceOptions();
            configure(options);
            services.AddSingleton(options);

            if (!options.EnableOutputCaching)
                return services;

            // 2. Configure Native Output Cache (Policies)
            // This registers the core services and defines the "Base Policy".
            services.AddOutputCache(baseOptions =>
            {
                // Define a "Default" policy that controllers can use implicitly or explicitly
                baseOptions.AddBasePolicy(builder => builder
                    .Expire(TimeSpan.FromSeconds(options.DefaultExpirationSeconds)));
            });

            // 3. Configure Persistence (Redis)
            // This registers IOutputCacheStore. If skipped, it defaults to InMemory.
            // NOTE: This is a separate service call, not part of AddOutputCache options.
            if (!string.IsNullOrEmpty(options.RedisConnectionString))
            {
                services.AddStackExchangeRedisOutputCache(redisOptions =>
                {
                    redisOptions.Configuration = options.RedisConnectionString;
                    redisOptions.InstanceName = "OutputCache:";
                });
            }

            return services;
        }

        /// <summary>
        /// Activates the OutputCache middleware.
        /// </summary>
        public static WebApplication UseSharedWebPerformance(this WebApplication app)
        {
            var options = app.Services.GetRequiredService<WebPerformanceOptions>();

            if (options.EnableOutputCaching)
            {
                app.UseOutputCache();
            }

            return app;
        }
    }
}