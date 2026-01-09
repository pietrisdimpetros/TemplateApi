namespace Shared.RateLimiting.Options
{
    public sealed class RateLimitingOptions
    {
        /// <summary>
        /// Gets or sets the name of the default policy to apply.
        /// Default: "StandardPolicy".
        /// </summary>
        public string PolicyName { get; set; } = "StandardPolicy";

        /// <summary>
        /// Gets or sets the maximum number of requests allowed within the window.
        /// Default: 100.
        /// </summary>
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// Gets or sets the time window in seconds for the limit.
        /// Default: 60 seconds.
        /// </summary>
        public int WindowSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the maximum number of queued requests when the limit is exceeded.
        /// Default: 0 (Reject immediately).
        /// </summary>
        public int QueueLimit { get; set; } = 0;
    }
}