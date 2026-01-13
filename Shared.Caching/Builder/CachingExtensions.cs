using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Caching.Options;
using Shared.Caching.Services;
using StackExchange.Redis;

namespace Shared.Caching.Builder
{
    public static class CachingExtensions
    {
        public static IServiceCollection AddSharedCaching(
            this IServiceCollection services,
            Action<CachingOptions> configure)
        {
            var options = new CachingOptions
            {
                ConnectionString = null,
                InstanceName = "Default:"
            };
            configure(options);
            services.AddSingleton(options);

            // 1. Configure L2 Cache (Redis) & HybridCache
            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                // 1. Production / Distributed (Redis)
                services.AddStackExchangeRedisCache(redisOptions =>
                {
                    redisOptions.Configuration = options.ConnectionString;
                    redisOptions.InstanceName = options.InstanceName;
                });
            }
            else
            {
                // 2. Development / Single Instance (In-Memory)
                services.AddDistributedMemoryCache();
            }
            // 2. Hybrid Cache
            services.AddHybridCache(hybridOptions =>
            {
                hybridOptions.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(options.DefaultExpirationMinutes),
                    LocalCacheExpiration = TimeSpan.FromMinutes(options.DefaultExpirationMinutes) // L1
                };
            });

            // 3. Register the Service Wrapper
            // We pass the global serializers to ensure AOT contexts are respected,
            // but HybridCache manages the actual serialization flow.
            services.AddSingleton<ICacheService, CacheService>();

            // 4. Data Protection (Persisting Keys to Redis)
            // This remains separate from HybridCache as it deals with specific Security keys.
            if (options.DataProtection != null &&
                 options.DataProtection.Enabled &&
                 !string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                var redisConnStr = !string.IsNullOrEmpty(options.DataProtection.ConnectionStringOverride)
                    ? options.DataProtection.ConnectionStringOverride
                    : options.ConnectionString;

                var redis = ConnectionMultiplexer.Connect(redisConnStr!);

                services.AddDataProtection()
                    .SetApplicationName(options.DataProtection.ApplicationName)
                    .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
            }

            return services;
        }
    }
}