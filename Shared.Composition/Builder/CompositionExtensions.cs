using Microsoft.Extensions.DependencyInjection;
using Shared.Composition.Installers;
using Shared.Composition.Options;
using Shared.Data.Abstractions;
using Shared.Data.Builder;

namespace Shared.Composition.Builder
{
    public static class CompositionExtensions
    {
        // Define the explicit execution order for the installers.
        // This static list avoids expensive reflection scanning at startup.
        private static readonly IReadOnlyList<IInfrastructureInstaller> Installers =
        [
            // 1. Observability & Diagnostics (Start early to catch startup issues)
            new LoggingInstaller(),
            new ErrorHandlingInstaller(),
            new TelemetryInstaller(),

            // 2. Core Infrastructure
            new CachingInstaller(),         

            // 3. Traffic & Security
            new IdempotencyInstaller(),    
            new SecurityInstaller(),
            new RateLimitingInstaller(),
            new WebPerformanceInstaller(),

            // 4. Feature Management & Docs
            new FeatureManagementInstaller(),
            new OpenApiInstaller(),

            // 5. Data Formatting
            new SerializationInstaller(),

            // 6. Health Checks (Builder registration)
            new HealthInstaller(),

            // 7. Connectivity & External
            new NetworkingInstaller(),

            // 8. Identity & Persistence
            new IdentityInstaller(),

            // 9. Global Essentials (Controllers, HttpContext, User)
            new EssentialsInstaller()
        ];

        /// <summary>
        /// Bootstraps the Shared Infrastructure using the Installer Pattern.
        /// </summary>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            Action<SharedInfrastructureOptions> configure,
            Action<IHealthChecksBuilder>? extraHealthChecks = null)
        {
            // 1. Evaluate and Register Options
            var rootOptions = new SharedInfrastructureOptions();
            configure(rootOptions);
            services.AddSingleton(rootOptions);

            // 2. Execute Installers
            foreach (var installer in Installers)
            {
                // Special handling for HealthInstaller which accepts the extra checks callback
                if (installer is HealthInstaller healthInstaller)
                    healthInstaller.Install(services, rootOptions, extraHealthChecks);
                else
                    installer.Install(services, rootOptions);
            }

            return services;
        }

        /// <summary>
        /// Registers a module-specific DbContext using the shared infrastructure settings.
        /// This remains here as a helper for the API project.
        /// </summary>
        public static IServiceCollection AddModuleDbContext<TContext>(
            this IServiceCollection services,
            string schemaName)
            where TContext : ModuleDbContext
        {
            services.AddModuleDatabase<TContext>(sp =>
            {
                var rootOptions = sp.GetRequiredService<SharedInfrastructureOptions>();

                if (rootOptions.Database is null)
                    throw new InvalidOperationException("Database options are missing in the shared infrastructure configuration.");

                // Map the shared options to the specific Database options
                return new Data.Options.DatabaseOptions
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