namespace Shared.Telemetry.Options
{
    public sealed class TelemetryOptions
    {
        /// <summary>
        /// Gets or sets the logical name of the service (e.g., "PaymentService").
        /// </summary>
        public required string ServiceName { get; set; }

        /// <summary>
        /// Gets or sets the service version (e.g., "1.0.0").
        /// </summary>
        public required string ServiceVersion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to export telemetry to Azure Monitor.
        /// If false, telemetry uses the OTLP exporter (standard for local .NET Aspire/Docker).
        /// </summary>
        public required bool UseAzureMonitor { get; set; }

        /// <summary>
        /// Gets or sets the OTLP Endpoint for local development (default is usually http://localhost:4317).
        /// Only used if UseAzureMonitor is false.
        /// </summary>
        public string OtlpEndpoint { get; set; } = "http://localhost:4317";

        // Agnostic list of sources to listen to
        public IList<string> ActivitySources { get; } = [];
    }
}