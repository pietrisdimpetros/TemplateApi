namespace Shared.Health.Options
{
    public sealed class HealthOptions
    {
        /// <summary>
        /// Gets or sets the URL endpoint for the Liveness probe.
        /// Default: "/health/live"
        /// </summary>
        public string LivenessEndpoint { get; set; } = "/health/live";

        /// <summary>
        /// Gets or sets the URL endpoint for the Readiness probe.
        /// Default: "/health/ready"
        /// </summary>
        public string ReadinessEndpoint { get; set; } = "/health/ready";

        /// <summary>
        /// Gets or sets a value indicating whether to enable the background publisher
        /// that logs health status periodically.
        /// </summary>
        public bool EnableLogPublisher { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to add the default "self" (Liveness) check.
        /// Set to false if you want to define your own liveness logic completely from scratch.
        /// </summary>
        public bool EnableDefaultHealthCheck { get; set; } = true;
    }
}