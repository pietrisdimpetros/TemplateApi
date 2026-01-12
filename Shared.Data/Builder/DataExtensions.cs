using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Shared.Data.Abstractions;
using Shared.Data.Interceptors;
using Shared.Data.Options;

namespace Shared.Data.Builder
{
    public static class DataExtensions
    {
        /// <summary>
        /// Registers a module-specific DbContext with standard enterprise resilience and pooling.
        /// </summary>
        public static IServiceCollection AddModuleDatabase<TContext>(
            this IServiceCollection services,
            Func<IServiceProvider, DatabaseOptions> optionsFactory,
            string schemaName)
            where TContext : ModuleDbContext
        {
            // 1. Register TimeProvider (Native .NET 8/9/10 feature)
            services.AddSingleton(TimeProvider.System);

            // 2. Register Interceptors
            services.TryAddSingleton<AuditingInterceptor>();
            services.TryAddSingleton<SoftDeleteInterceptor>();

            services.TryAddSingleton(sp => new SlowQueryInterceptor(
                sp.GetRequiredService<ILogger<SlowQueryInterceptor>>(),
                optionsFactory(sp).SlowQueryThresholdMilliseconds)
            );

            // Native First: We use AddDbContextPool for performance (reusing context instances).
            services.AddDbContextPool<TContext>((serviceProvider, dbOptions) =>
            {
                // 3. Resolve Options using the Factory (Inside the scope)
                var options = optionsFactory(serviceProvider);

                // 4. Configure SQL Server
                dbOptions.UseSqlServer(options.ConnectionString, sqlOptions =>
                {
                    // CONNECTION RESILIENCY (The "Retry Policy")
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: options.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(options.MaxRetryDelaySeconds),
                        errorNumbersToAdd: null);

                    // Command Timeout
                    sqlOptions.CommandTimeout(options.CommandTimeoutSeconds);

                    // Migrations History Isolation
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schemaName);
                });

                // --- WIRE INTERCEPTORS ---
                if (options.EnableSoftDelete)
                    dbOptions.AddInterceptors(serviceProvider.GetRequiredService<SoftDeleteInterceptor>());

                if (options.EnableAuditing)
                    dbOptions.AddInterceptors(serviceProvider.GetRequiredService<AuditingInterceptor>());

                if (options.EnableSlowQueryLogging)
                    dbOptions.AddInterceptors(serviceProvider.GetRequiredService<SlowQueryInterceptor>());

                // 5. Dev/Debug Settings
                if (options.EnableDetailedErrors)
                    dbOptions.EnableDetailedErrors();

                if (options.EnableSensitiveDataLogging)
                    dbOptions.EnableSensitiveDataLogging();
            });

            return services;
        }
    }
}