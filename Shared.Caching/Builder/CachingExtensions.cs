using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Shared.Caching.Options;
using Shared.Caching.Services;
using StackExchange.Redis;
namespace Shared.Caching.Builder
{
    public static class CachingExtensions
    {
        /// <summary>
        /// Adds Shared.Caching infrastructure.
        /// Configures StackExchange.Redis and registers the typed ICacheService.
        /// </summary>
        public static IServiceCollection AddSharedCaching(
            this IServiceCollection services,
            Action<CachingOptions> configure)
        {
            // 1. Configure Options
            var options = new CachingOptions
            {
                ConnectionString = "localhost:6379",
                InstanceName = "Default:"
            };
            configure(options);

            services.AddSingleton(options);

            // 2. Configure Native IDistributedCache (Redis)
            services.AddStackExchangeRedisCache(redisOptions =>
            {
                redisOptions.Configuration = options.ConnectionString;
                redisOptions.InstanceName = options.InstanceName;
            });

            // 3. Register the Typed Cache Service
            services.AddSingleton<ICacheService, CacheService>();

            // 4. Configure Distributed Data Protection (Redis)
            if (options.DataProtection != null && options.DataProtection.Enabled)
            {
                var redisConnStr = !string.IsNullOrEmpty(options.DataProtection.ConnectionStringOverride)
                    ? options.DataProtection.ConnectionStringOverride
                    : options.ConnectionString;

                var redis = ConnectionMultiplexer.Connect(redisConnStr);

                services.AddDataProtection()
                    .SetApplicationName(options.DataProtection.ApplicationName)
                    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            }


            return services;
        }
    }
}