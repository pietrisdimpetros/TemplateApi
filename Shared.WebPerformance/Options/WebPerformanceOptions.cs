namespace Shared.WebPerformance.Options
{
    public sealed class WebPerformanceOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether Output Caching is enabled.
        /// Default: true.
        /// </summary>
        public bool EnableOutputCaching { get; set; } = true;

        /// <summary>
        /// Gets or sets the default expiration time in seconds for cached responses.
        /// Default: 60 seconds.
        /// </summary>
        public int DefaultExpirationSeconds { get; set; } = 60;

        /// <summary>
        /// Gets or sets the Redis connection string for distributed output caching.
        /// If null, the cache will be stored in-memory (not recommended for multi-pod setups).
        /// Example: "localhost:6379,abortConnect=false"
        /// </summary>
        public string? RedisConnectionString { get; set; }
    }
}