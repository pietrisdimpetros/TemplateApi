using Shared.Caching.Options;
using Shared.Data.Options;
using Shared.ErrorHandling.Options;
using Shared.FeatureManagement.Options;
using Shared.Health.Options;
using Shared.Logging.Options;
using Shared.Networking.Options;
using Shared.RateLimiting.Options;
using Shared.Resilience.Options;
using Shared.Security.Options;
using Shared.Serialization.Options;
using Shared.Swagger.Options;
using Shared.Telemetry.Options;
using Shared.WebPerformance.Options;

namespace Shared.Composition.Options
{
    /// <summary>
    /// The master configuration object.
    /// Properties are Nullable: if a property is null, that feature is NOT registered.
    /// We reuse the specific Option classes to avoid duplication and enforce strict typing.
    /// </summary>
    public sealed class SharedInfrastructureOptions
    {
        public CachingOptions? Caching { get; set; }
        public LoggingOptions? Logging { get; set; }
        public ErrorHandlingOptions? ErrorHandling { get; set; }
        public TelemetryOptions? Telemetry { get; set; }
        public OpenApiOptions? OpenApi { get; set; }
        public SerializationOptions? Serialization { get; set; }
        public SecurityOptions? Security { get; set; }
        public NetworkingOptions? Networking { get; set; }
        public HealthOptions? Health { get; set; }
        public ResilienceOptions? Resilience { get; set; }
        public RateLimitingOptions? RateLimiting { get; set; }
        public WebPerformanceOptions? WebPerformance { get; set; }
        public FeatureManagementOptions? FeatureManagement { get; set; }
        public DatabaseOptions? Database { get; set; }
    }
}