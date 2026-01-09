using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
            DatabaseOptions options,
            string schemaName)
            where TContext : ModuleDbContext
        {
            // 1. Register TimeProvider (Native .NET 8/9/10 feature)
            services.AddSingleton(TimeProvider.System);

            // 2. Register Interceptors as Singletons (they are stateless)
            if (options.EnableAuditing)
                services.AddScoped<AuditingInterceptor>();

            if (options.EnableSoftDelete)
                services.AddSingleton<SoftDeleteInterceptor>();

            if (options.EnableSlowQueryLogging)
            {
                services.AddSingleton(sp => new SlowQueryInterceptor(
                    sp.GetRequiredService<ILogger<SlowQueryInterceptor>>(),
                    options.SlowQueryThresholdMilliseconds));
            }
            
            // Native First: We use AddDbContextPool for performance (reusing context instances).
            services.AddDbContextPool<TContext>((serviceProvider, dbOptions) =>
            {
                // 1. Configure SQL Server
                dbOptions.UseSqlServer(options.ConnectionString, sqlOptions =>
                {
                    // 2. CONNECTION RESILIENCY (The "Retry Policy")
                    // Automatically retries on transient errors (deadlocks, timeouts, network blips).
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: options.MaxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(options.MaxRetryDelaySeconds),
                        errorNumbersToAdd: null); // Use default SQL transient error codes

                    // 3. Command Timeout
                    sqlOptions.CommandTimeout(options.CommandTimeoutSeconds);

                    // 4. Migrations History Isolation
                    // We keep the history table in the specific schema to avoid conflicts if modules manage their own migrations.
                    // Note: We need to instantiate TContext to access the abstract Schema property 
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schemaName);
                });

                // --- WIRE INTERCEPTORS ---
                if (options.EnableAuditing)
                    dbOptions.AddInterceptors(serviceProvider.GetRequiredService<AuditingInterceptor>());

                if (options.EnableSoftDelete)
                    dbOptions.AddInterceptors(serviceProvider.GetRequiredService<SoftDeleteInterceptor>());

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