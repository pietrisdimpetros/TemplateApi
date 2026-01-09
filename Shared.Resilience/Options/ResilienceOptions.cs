namespace Shared.Resilience.Options
{
    public sealed class ResilienceOptions
    {
        /// <summary>
        /// Gets or sets the number of retry attempts.
        /// Default is 3.
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the base delay between retries in seconds.
        /// Default is 2 seconds (Exponential backoff applies).
        /// </summary>
        public int RetryDelaySeconds { get; set; } = 2;

        /// <summary>
        /// Gets or sets the number of failures allowed before the circuit breaks.
        /// Default is 5.
        /// </summary>
        public int CircuitBreakerThreshold { get; set; } = 5;

        /// <summary>
        /// Gets or sets the duration in seconds to keep the circuit open before testing recovery.
        /// Default is 10 seconds.
        /// </summary>
        public int CircuitBreakerBreakDurationSeconds { get; set; } = 10;

        /// <summary>
        /// Gets or sets the total request timeout in seconds.
        /// Default is 30 seconds.
        /// </summary>
        public int TotalRequestTimeoutSeconds { get; set; } = 30;
    }
}