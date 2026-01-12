using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Caching.Options;
using Shared.Caching.Services;
using StackExchange.Redis;
using System.Text.Json;

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

            // --- CHANGED: Explicitly configure Cache Serialization Stability ---
            services.AddSingleton<ICacheService>(sp =>
            {
                var distributedCache = sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
                var cacheOptions = sp.GetRequiredService<IOptions<CachingOptions>>();

                // 1. Get the Global Options to reuse the registered AOT Contexts (TypeResolvers)
                // We want the knowledge of *types* (Product, WeatherForecast) from the global app,
                // but we DO NOT want the global *formatting policies*.
                var globalJsonOptions = sp.GetRequiredService<JsonSerializerOptions>();

                // 2. Create a Dedicated Options instance for Persistence
                var stableOptions = new JsonSerializerOptions
                {
                    // FORCE CamelCase. Even if the API changes to SnakeCase for example, 
                    // the data in Redis remains readable and consistent.
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,

                    // Standard persistence settings
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = false,

                    // REUSE the TypeInfoResolver from global config so Source Generators still work
                    TypeInfoResolver = globalJsonOptions.TypeInfoResolver
                };

                stableOptions.MakeReadOnly();

                return new CacheService(distributedCache, cacheOptions, stableOptions);
            });
            // ------------------------------------------------------------------

            if (options.DataProtection != null &&
                 options.DataProtection.Enabled &&
                 !string.IsNullOrWhiteSpace(options.ConnectionString)) // Don't try to persist keys if using Memory
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