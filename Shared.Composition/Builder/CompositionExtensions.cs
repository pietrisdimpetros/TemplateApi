using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Caching.Builder;
// CompositionHelper is no longer used/needed
using Shared.Composition.Options;
using Shared.Composition.Services;
using Shared.Data.Abstractions;
using Shared.Data.Builder;
using Shared.ErrorHandling.Builder;
using Shared.FeatureManagement.Builder;
using Shared.Health.Builder;
using Shared.Health.Internal;
using Shared.Identity.Builder;
using Shared.Logging.Builder;
using Shared.Networking.Builder;
using Shared.RateLimiting.Builder;
using Shared.Security.Builder;
using Shared.Serialization.Builder;
using Shared.Swagger.Builder;
using Shared.Telemetry.Builder;
using Shared.WebPerformance.Builder;

namespace Shared.Composition.Builder
{
    public static class CompositionExtensions
    {
        /// <summary>
        /// Bootstraps the Shared Infrastructure based on the provided configuration.
        /// Only libraries with non-null options in the configuration action will be registered.
        /// </summary>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            Action<SharedInfrastructureOptions> configure,
            Action<IHealthChecksBuilder>? extraHealthChecks = null)
        {
            // 1. Evaluate the user's configuration
            var rootOptions = new SharedInfrastructureOptions();
            configure(rootOptions);
            services.AddSingleton(rootOptions);

            // 2. Conditionally Register Services using Manual Assignment (AOT Safe)

            // Logging
            if (rootOptions.Logging is not null)
            {
                services.AddSharedLogging(opt =>
                {
                    opt.EnableDetailedOutput = rootOptions.Logging.EnableDetailedOutput;
                    opt.EnableEnrichment = rootOptions.Logging.EnableEnrichment;
                });
            }

            // Error Handling
            if (rootOptions.ErrorHandling is not null)
            {
                services.AddSharedErrorHandling(opt =>
                {
                    opt.IncludeStackTrace = rootOptions.ErrorHandling.IncludeStackTrace;
                });
            }

            // Telemetry
            if (rootOptions.Telemetry is not null)
            {
                services.AddSharedTelemetry(opt =>
                {
                    opt.ServiceName = rootOptions.Telemetry.ServiceName;
                    opt.ServiceVersion = rootOptions.Telemetry.ServiceVersion;
                    opt.UseAzureMonitor = rootOptions.Telemetry.UseAzureMonitor;
                    opt.OtlpEndpoint = rootOptions.Telemetry.OtlpEndpoint;
                });
            }

            // Caching
            if (rootOptions.Caching is not null)
            {
                services.AddSharedCaching(opt =>
                {
                    opt.ConnectionString = rootOptions.Caching.ConnectionString;
                    opt.InstanceName = rootOptions.Caching.InstanceName;
                    opt.DefaultExpirationMinutes = rootOptions.Caching.DefaultExpirationMinutes;
                    opt.DataProtection = rootOptions.Caching.DataProtection;
                });
            }

            // Security
            if (rootOptions.Security is not null)
            {
                services.AddSharedSecurity(opt =>
                {
                    opt.Authority = rootOptions.Security.Authority;
                    opt.Audience = rootOptions.Security.Audience;
                    opt.AllowedOrigins = rootOptions.Security.AllowedOrigins;
                });
            }

            // Rate Limiting
            if (rootOptions.RateLimiting is not null)
            {
                services.AddSharedRateLimiting(opt =>
                {
                    opt.PolicyName = rootOptions.RateLimiting.PolicyName;
                    opt.PermitLimit = rootOptions.RateLimiting.PermitLimit;
                    opt.WindowSeconds = rootOptions.RateLimiting.WindowSeconds;
                    opt.QueueLimit = rootOptions.RateLimiting.QueueLimit;
                });
            }

            // Web Performance (Output Caching)
            if (rootOptions.WebPerformance is not null)
            {
                services.AddSharedWebPerformance(opt =>
                {
                    opt.EnableOutputCaching = rootOptions.WebPerformance.EnableOutputCaching;
                    opt.DefaultExpirationSeconds = rootOptions.WebPerformance.DefaultExpirationSeconds;
                    opt.RedisConnectionString = rootOptions.WebPerformance.RedisConnectionString;
                });
            }

            // Feature Management
            if (rootOptions.FeatureManagement is not null)
            {
                services.AddSharedFeatureManagement(opt =>
                {
                    opt.FailIfMissing = rootOptions.FeatureManagement.FailIfMissing;
                    opt.SectionName = rootOptions.FeatureManagement.SectionName;
                });
            }

            // OpenAPI/Swagger
            if (rootOptions.OpenApi is not null)
            {
                services.AddSharedOpenApi(opt =>
                {
                    opt.DocumentTitle = rootOptions.OpenApi.DocumentTitle;
                    opt.DocumentVersion = rootOptions.OpenApi.DocumentVersion;
                    opt.EnableAuth = rootOptions.OpenApi.EnableAuth;
                });
            }

            // Serialization
            if (rootOptions.Serialization is not null)
            {
                // 1. Register the Health module's AOT context into the global chain
                rootOptions.Serialization.TypeInfoResolverChain.Add(HealthJsonContext.Default);

                // 2. Register the service normally
                services.AddSharedSerialization(opt =>
                {
                    opt.NamingPolicy = rootOptions.Serialization.NamingPolicy;
                    opt.IgnoreCondition = rootOptions.Serialization.IgnoreCondition;
                    opt.WriteIndented = rootOptions.Serialization.WriteIndented;
                    // Note: TypeInfoResolverChain is a list and handled by reference if we wanted to copy it,
                    // but AddSharedSerialization handles the registration of the chain elements separately.
                });
            }

            // Health Checks
            if (rootOptions.Health is not null)
            {
                // 1. Register base infrastructure
                var healthBuilder = services.AddSharedHealth(opt =>
                {
                    opt.LivenessEndpoint = rootOptions.Health.LivenessEndpoint;
                    opt.ReadinessEndpoint = rootOptions.Health.ReadinessEndpoint;
                    opt.EnableLogPublisher = rootOptions.Health.EnableLogPublisher;
                    opt.EnableDefaultHealthCheck = rootOptions.Health.EnableDefaultHealthCheck;
                });

                // 2. Inject Business Logic
                extraHealthChecks?.Invoke(healthBuilder);
            }

            // Networking
            if (rootOptions.Networking is not null)
            {
                services.AddSharedNetworking(opt =>
                {
                    opt.UserAgent = rootOptions.Networking.UserAgent;
                    opt.TimeoutSeconds = rootOptions.Networking.TimeoutSeconds;
                    opt.MaxRetries = rootOptions.Networking.MaxRetries;
                    opt.IgnoreSslErrors = rootOptions.Networking.IgnoreSslErrors;
                });
            }

            services.AddHttpContextAccessor();
            services.TryAddSingleton<ICurrentUserService,WebCurrentUserService>();

            // Identity
            if (rootOptions.Database != null && rootOptions.Identity != null)
            {
                services.AddSharedIdentity(opt =>
                {
                    opt.ConnectionString = rootOptions.Database.ConnectionString;
                    opt.EnableDetailedErrors = rootOptions.Database.EnableDetailedErrors;
                    opt.SchemaName = "identity";
                    opt.MaxRetryCount = rootOptions.Database.MaxRetryCount;
                    opt.MaxRetryDelaySeconds = rootOptions.Database.MaxRetryDelaySeconds;
                    opt.CommandTimeoutSeconds = rootOptions.Database.CommandTimeoutSeconds;
                });
            }   

            return services;
        }

        public static IServiceCollection AddModuleDbContext<TContext>(
            this IServiceCollection services,
            string schemaName)
            where TContext : ModuleDbContext
        {
            services.AddModuleDatabase<TContext>(sp =>
            {
                var rootOptions = sp.GetRequiredService<SharedInfrastructureOptions>();

                if (rootOptions.Database is null)
                    throw new InvalidOperationException("Database options are missing.");

                return new Shared.Data.Options.DatabaseOptions
                {
                    ConnectionString = rootOptions.Database.ConnectionString,
                    MaxRetryCount = rootOptions.Database.MaxRetryCount,
                    MaxRetryDelaySeconds = rootOptions.Database.MaxRetryDelaySeconds,
                    CommandTimeoutSeconds = rootOptions.Database.CommandTimeoutSeconds,
                    EnableDetailedErrors = rootOptions.Database.EnableDetailedErrors,
                    EnableSensitiveDataLogging = rootOptions.Database.EnableSensitiveDataLogging,
                    EnableAuditing = rootOptions.Database.EnableAuditing,
                    EnableSoftDelete = rootOptions.Database.EnableSoftDelete,
                    EnableSlowQueryLogging = rootOptions.Database.EnableSlowQueryLogging,
                    SlowQueryThresholdMilliseconds = rootOptions.Database.SlowQueryThresholdMilliseconds
                };
            }, schemaName);

            return services;
        }
    }
}