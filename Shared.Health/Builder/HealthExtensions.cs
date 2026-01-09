using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Health.Internal;
using Shared.Health.Options;
namespace Shared.Health.Builder
{
    public static class HealthExtensions
    {
        /// <summary>
        /// Adds Shared.Health infrastructure.
        /// STRICTLY INFRASTRUCTURE: No business logic checks are added here.
        /// </summary>
        public static IHealthChecksBuilder AddSharedHealth(
            this IServiceCollection services,
            Action<HealthOptions> configure)
        {
            // 1. Configure Options
            var options = new HealthOptions();
            configure(options);
            services.AddSingleton(options);

            // 2. Initialize the Builder
            var builder = services.AddHealthChecks();

            // 3. Add Default "Self" Check (Optional)
            // This is the only "logic" the library owns, and it can be turned off.
            if (options.EnableDefaultHealthCheck)
                builder.AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

            // 4. Add Log Publisher (Optional)
            if (options.EnableLogPublisher)
                services.AddSingleton<IHealthCheckPublisher, LogHealthCheckPublisher>();

            return builder;
        }

        /// <summary>
        /// Maps the Liveness and Readiness health check endpoints.
        /// </summary>
        public static WebApplication UseSharedHealth(this WebApplication app)
        {
            var options = app.Services.GetRequiredService<HealthOptions>();

            // 1. Map Liveness (Runs only "live" tagged checks)
            // Usually just the "self" check. Fast.
            app.MapHealthChecks(options.LivenessEndpoint, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live"),
                ResponseWriter = WriteJsonHealthResponse
            });

            // 2. Map Readiness (Runs "ready" tagged checks)
            // This is where DB, Cache, External API checks should run.
            // NOTE: Consuming services must add their checks with the "ready" tag.
            app.MapHealthChecks(options.ReadinessEndpoint, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("ready"),
                ResponseWriter = WriteJsonHealthResponse
            });

            return app;
        }

        /// <summary>
        /// Writes a detailed JSON response for the health check.
        /// </summary>
        private static Task WriteJsonHealthResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration,
                results = report.Entries.ToDictionary(
                    e => e.Key,
                    e => new
                    {
                        status = e.Value.Status.ToString(),
                        description = e.Value.Description,
                        data = e.Value.Data
                    })
            };

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}