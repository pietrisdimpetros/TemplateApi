using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Shared.Logging.Sql.Abstractions;
using Shared.Logging.Sql.Infrastructure;
using Shared.Logging.Sql.Internal;
using Shared.Logging.Sql.Options;
using Shared.Logging.Sql.Providers;
using Shared.Logging.Sql.Services;

namespace Shared.Logging.Sql.Builder
{
    public static class SqlLoggingExtensions
    {
        /// <summary>
        /// Registers the SQL Logging module using a dynamic connection source strategy.
        /// </summary>
        public static IServiceCollection AddSharedSqlLogging(
            this IServiceCollection services,
            Func<IServiceProvider, string?> connectionStringFactory,
            Action<SqlLoggingOptions>? configureOptions = null)
        {
            var optionsBuilder = services.AddOptions<SqlLoggingOptions>();
            if (configureOptions != null) optionsBuilder.Configure(configureOptions);

            // 1. Register the Strategy
            services.TryAddSingleton<ISqlConnectionSource>(sp =>
                new DelegateConnectionSource(sp, connectionStringFactory));

            // 2. Core Infrastructure
            services.TryAddSingleton<LogBuffer>();

            // 3. Background Services
            services.AddHostedService<SqlLogInitializer>();
            services.AddHostedService<SqlLogProcessor>();

            // 4. Provider
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, SqlLoggerProvider>());

            return services;
        }

        private sealed class DelegateConnectionSource(
            IServiceProvider sp,
            Func<IServiceProvider, string?> factory) : ISqlConnectionSource
        {
            public Task<string?> GetConnectionStringAsync(CancellationToken ct)
                => Task.FromResult(factory(sp));
        }
    }
}