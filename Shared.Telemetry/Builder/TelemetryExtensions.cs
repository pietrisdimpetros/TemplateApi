using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Telemetry.Options;
namespace Shared.Telemetry.Builder
{
    public static class TelemetryExtensions
    {
        /// <summary>
        /// Adds Shared.Telemetry infrastructure.
        /// Configures OpenTelemetry with either Azure Monitor or OTLP (fallback) exporters.
        /// Includes Tracing, Metrics, and Logging bridge.
        /// </summary>
        public static IServiceCollection AddSharedTelemetry(
            this IServiceCollection services,
            Action<TelemetryOptions> configure)
        {
            // 1. Configure Options
            var options = new TelemetryOptions
            {
                ServiceName = "UnknownService",
                ServiceVersion = "0.0.0",
                UseAzureMonitor = false
            };
            configure(options);

            services.AddSingleton(options);

            // 2. Build the Resource Definition (Service Name/Version)
            Action<ResourceBuilder> configureResource = r => r
                .AddService(
                    serviceName: options.ServiceName,
                    serviceVersion: options.ServiceVersion,
                    serviceInstanceId: Environment.MachineName);

            // 3. Configure OpenTelemetry
            var otelBuilder = services.AddOpenTelemetry()
                .ConfigureResource(configureResource)
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation();

                    if (!options.UseAzureMonitor)
                    {
                        tracing.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(options.OtlpEndpoint));
                    }
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();

                    if (!options.UseAzureMonitor)
                    {
                        metrics.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(options.OtlpEndpoint));
                    }
                });

            // 4. Azure Monitor Configuration (Distro)
            // The Azure Monitor Distro automatically configures Traces, Metrics, and Logs.
            if (options.UseAzureMonitor)
            {
                otelBuilder.UseAzureMonitor();
            }

            // 5. Configure Logging Bridge (Native ILogger -> OpenTelemetry)
            // If using Azure Monitor, UseAzureMonitor() handles this. 
            // If using OTLP, we must manually wire it up.
            if (!options.UseAzureMonitor)
            {
                services.AddLogging(logging =>
                {
                    logging.AddOpenTelemetry(otelLogger =>
                    {
                        otelLogger.IncludeScopes = true;
                        otelLogger.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(options.ServiceName));
                        otelLogger.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(options.OtlpEndpoint));
                    });
                });
            }

            return services;
        }
    }
}