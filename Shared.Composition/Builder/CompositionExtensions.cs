using Microsoft.Extensions.DependencyInjection;
using Shared.Caching.Builder;
using Shared.Composition.Helper;
using Shared.Composition.Options;
using Shared.ErrorHandling.Builder;
using Shared.FeatureManagement.Builder;
using Shared.Health.Builder;
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
            // 2. Conditionally Register Services using Reflection for property mapping

            // Logging
            if (rootOptions.Logging is not null)
                services.AddSharedLogging(opt => CompositionHelper.CopyProperties(rootOptions.Logging, opt));

            // Error Handling
            if (rootOptions.ErrorHandling is not null)
                services.AddSharedErrorHandling(opt => CompositionHelper.CopyProperties(rootOptions.ErrorHandling, opt));

            // Telemetry
            if (rootOptions.Telemetry is not null)
                services.AddSharedTelemetry(opt => CompositionHelper.CopyProperties(rootOptions.Telemetry, opt));

            // Caching
            if (rootOptions.Caching is not null)
                services.AddSharedCaching(opt => CompositionHelper.CopyProperties(rootOptions.Caching, opt));

            // Security
            if (rootOptions.Security is not null)
                services.AddSharedSecurity(opt => CompositionHelper.CopyProperties(rootOptions.Security, opt));

            // Rate Limiting
            if (rootOptions.RateLimiting is not null)
                services.AddSharedRateLimiting(opt => CompositionHelper.CopyProperties(rootOptions.RateLimiting, opt));

            // Web Performance (Output Caching)
            if (rootOptions.WebPerformance is not null)
                services.AddSharedWebPerformance(opt => CompositionHelper.CopyProperties(rootOptions.WebPerformance, opt));

            // Feature Management
            if (rootOptions.FeatureManagement is not null)
                services.AddSharedFeatureManagement(opt => CompositionHelper.CopyProperties(rootOptions.FeatureManagement, opt));

            // OpenAPI/Swagger
            if (rootOptions.OpenApi is not null)
                services.AddSharedOpenApi(opt => CompositionHelper.CopyProperties(rootOptions.OpenApi, opt));

            // Serialization
            if (rootOptions.Serialization is not null)
                services.AddSharedSerialization(opt => CompositionHelper.CopyProperties(rootOptions.Serialization, opt));

            // Health Checks
            if (rootOptions.Health is not null)
            {
                // 1. Register base infrastructure (The Mechanism)
                // This adds the "self" check (if enabled) and the Publisher
                var healthBuilder = services.AddSharedHealth(opt =>
                    CompositionHelper.CopyProperties(rootOptions.Health, opt));

                // 2. Inject Business Logic (The Policy)
                // This applies the checks defined in Program.cs
                extraHealthChecks?.Invoke(healthBuilder);
            }

            // Networking
            if (rootOptions.Networking is not null)
                services.AddSharedNetworking(opt => CompositionHelper.CopyProperties(rootOptions.Networking, opt));

            // Note: Shared.Resilience is strictly for HttpClientBuilder extensions 
            // and usually doesn't need global service registration, but we keep the pattern 
            // if you decided to register a global registry in the future.

            return services;
        }
    }
}
