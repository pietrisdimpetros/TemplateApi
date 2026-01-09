using Shared.Composition.Builder;
using Shared.Resilience.Options;
using Shared.Serialization.Options;
using Shared.Telemetry.Options;
using TemplateApi.Buisness.Health.Checks;

namespace TemplateApi
{
    public partial class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ============================================================================
            // 1. INFRASTRUCTURE REGISTRATION
            // ============================================================================
            builder.Services.AddInfrastructure(
            options =>
            {
                // ------------------------------------------------------------------------
                // GLOBAL CONFIGURATION BINDING
                // ------------------------------------------------------------------------
                // Binds the "Infrastructure" section from appsettings.json to the options object.
                // This replaces the manual "new Option { ... }" assignments.
                builder.Configuration.GetSection("Infrastructure").Bind(options);

                // --- 1. Logging ---
                // JSON Console + Enrichment (MachineName, Environment)
                if (options.Logging != null)
                {
                    // Force detailed output if in Development, regardless of config
                    if (builder.Environment.IsDevelopment())
                    {
                        options.Logging.EnableDetailedOutput = true;
                    }
                }

                // --- 2. Telemetry ---
                // Metrics/Tracing via OpenTelemetry
                // (Configuration is fully handled via appsettings.json binding)
                // Fallback defaults if config is missing
                options.Telemetry ??= new TelemetryOptions
                {
                    ServiceName = "ReferenceApi",
                    ServiceVersion = "1.0.0-beta",
                    UseAzureMonitor = false,
                    OtlpEndpoint = "http://localhost:4317"
                };

                // --- 3. Caching ---
                // Redis Connection
                if (options.Caching != null)
                {
                    // Fallback: If ConnectionString is missing in Infrastructure config, 
                    // try the root "ConnectionStrings:Redis" section.
                    if (string.IsNullOrEmpty(options.Caching.ConnectionString))
                    {
                        options.Caching.ConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
                    }
                }

                // --- 4. Security ---
                // JWT Bearer Auth + CORS
                // (Configuration is fully handled via appsettings.json binding)

                // --- 5. Networking ---
                // "ResilientClient" with Retries, Circuit Breaker, Timeout
                if (options.Networking != null)
                {
                    // Force ignoring SSL errors in Development for local testing
                    if (builder.Environment.IsDevelopment())
                    {
                        options.Networking.IgnoreSslErrors = true;
                    }
                }

                // --- 6. OpenAPI ---
                // Swagger Documentation
                // (Configuration is fully handled via appsettings.json binding)

                // --- 7. Serialization ---
                // System.Text.Json (CamelCase, IgnoreNull)
                // Ensure defaults are present if config is missing
                options.Serialization ??= new SerializationOptions();

                // --- 8. Health Checks ---
                // Probes for K8s / Load Balancers
                // (Configuration is fully handled via appsettings.json binding)

                // --- 9. Error Handling ---
                // Global ProblemDetails
                if (options.ErrorHandling != null)
                {
                    // Force StackTrace inclusion in Development
                    if (builder.Environment.IsDevelopment())
                    {
                        options.ErrorHandling.IncludeStackTrace = true;
                    }
                }

                // 10. Resilience (Utility options usually used manually, but populated for completeness)
                options.Resilience ??= new ResilienceOptions();
            },
            healthBuilder =>
            {
                // Register your BL checks here
                healthBuilder.AddCheck<GraphFunctionalityCheck>("graph_functional_test", tags: ["ready"]);
                healthBuilder.AddCheck<DemoCheck>("demo_test", tags: ["demo", "ready"]);

                // Example: Add standard SQL check (if package installed)
                healthBuilder.AddSqlServer(
                    connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
                    name: "sql_server",
                    tags: ["ready"]
                );
            });

            // Add Controllers
            builder.Services.AddControllers();

            var app = builder.Build();

            // ============================================================================
            // 2. MIDDLEWARE PIPELINE
            // ============================================================================
            // It enforces the correct order (Error -> Auth -> Health).
            app.UseSharedInfrastructure();
            app.MapControllers();
            app.Run();
        }
    }
}