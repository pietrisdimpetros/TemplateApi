using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Shared.Logging.Enrichment;
using Shared.Logging.Options;
using System.Text.Json;
namespace Shared.Logging.Builder
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Adds the Shared.Logging infrastructure using native Microsoft.Extensions.Logging.
        /// Configures JSON Console logging with UTC timestamps and optional enrichment.
        /// </summary>
        public static IServiceCollection AddSharedLogging(
            this IServiceCollection services,
            Action<LoggingOptions> configure)
        {
            // 1. Configure Options
            var options = new LoggingOptions
            {
                EnableDetailedOutput = false,
                EnableEnrichment = true
            };
            configure(options);

            services.AddSingleton(options);

            // 2. Register Enrichment Service
            if (options.EnableEnrichment)
            {
                services.AddSingleton<ILogEnricher, LogEnricher>();
            }

            // 3. Configure Native Logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();

                builder.AddJsonConsole(console =>
                {
                    console.UseUtcTimestamp = true;
                    console.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
                });
            });

            // 4. Configure JSON Formatter Options (Post-Configuration)
            // This enforces scope inclusion and indentation based on options.
            services.Configure<JsonConsoleFormatterOptions>(formatterOptions =>
            {
                formatterOptions.IncludeScopes = options.EnableEnrichment;
                formatterOptions.UseUtcTimestamp = true;
                formatterOptions.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

                formatterOptions.JsonWriterOptions = new JsonWriterOptions
                {
                    Indented = options.EnableDetailedOutput
                };
            });

            return services;
        }
    }
}