namespace Shared.Caching.Options
{
    public sealed class CachingOptions
    {
        /// <summary>
        /// Gets or sets the Redis connection string.
        /// Example: "localhost:6379,abortConnect=false"
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the instance name to prefix keys with.
        /// Example: "PaymentService:"
        /// </summary>
        public required string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets the default expiration time in minutes for cache entries if not specified.
        /// Default is 60 minutes.
        /// </summary>
        public int DefaultExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// Configuration for Distributed Data Protection.
        /// If null, Data Protection logic is skipped in this library.
        /// </summary>
        public CachingDataProtectionOptions? DataProtection { get; set; }
    }
}