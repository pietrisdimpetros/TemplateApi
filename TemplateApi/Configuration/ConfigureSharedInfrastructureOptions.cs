using Microsoft.Extensions.Options;
using Shared.Composition.Options;
using Shared.Resilience.Options;
using Shared.Serialization.Options;
using Shared.Telemetry.Options;
using TemplateApi.Business.Constants;
using TemplateApi.Serialization;

namespace TemplateApi.Configuration
{
    public class ConfigureSharedInfrastructureOptions(
         IHostEnvironment environment,
         IConfiguration configuration)
         : IConfigureOptions<SharedInfrastructureOptions>
    {
        public void Configure(SharedInfrastructureOptions options)
        {
            // 1. Logging: Force detailed output in Development
            if (options.Logging != null && environment.IsDevelopment())
                options.Logging.EnableDetailedOutput = true;

            // 2. Telemetry: Fallback defaults if config is missing
            options.Telemetry ??= new TelemetryOptions
            {
                ServiceName = "ReferenceApi",
                ServiceVersion = "1.0.0-beta",
                UseAzureMonitor = false,
                OtlpEndpoint = "http://localhost:4317"
            };

            // 3. Caching: Fallback for ConnectionString
            if (options.Caching != null && string.IsNullOrEmpty(options.Caching.ConnectionString))
                options.Caching.ConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

            // 4. Networking: Dev-specific SSL relaxation
            if (options.Networking != null && environment.IsDevelopment())
                options.Networking.IgnoreSslErrors = true;

            // 5. Serialization: Register Source Generator Context
            options.Serialization ??= new SerializationOptions();
            options.Serialization.TypeInfoResolverChain.Add(ApiJsonContext.Default);

            // 6. Error Handling: Force StackTrace in Development
            if (options.ErrorHandling != null && environment.IsDevelopment())
                options.ErrorHandling.IncludeStackTrace = true;

            // 7. Resilience: Ensure utility options are populated
            options.Resilience ??= new ResilienceOptions();

            // 8. SQL Logging: Set Audit Schema constants (Moved from PostConfigure)
            if (options.SqlLogging != null)
            {
                options.SqlLogging.SchemaName = AuditConstants.Schema;
                options.SqlLogging.TableName = AuditConstants.Table;
            }
        }
    }
}
