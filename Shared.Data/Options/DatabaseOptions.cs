namespace Shared.Data.Options
{
    public sealed class DatabaseOptions
    {
        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        public required string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retry attempts for transient failures (Connection Resilience).
        /// Default: 3.
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the maximum delay between retry attempts in seconds.
        /// </summary>
        public int MaxRetryDelaySeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the command timeout in seconds.
        /// Default: 30.
        /// </summary>
        public int CommandTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets a value indicating whether to enable detailed error logging (Dev/Debug only).
        /// </summary>
        public bool EnableDetailedErrors { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to enable sensitive data logging (Dev/Debug only).
        /// </summary>
        public bool EnableSensitiveDataLogging { get; set; } = false;

        /// <summary>
        /// Enables automatic setting of CreatedAt/ModifiedAt/CreatedBy/ModifiedBy.
        /// </summary>
        public bool EnableAuditing { get; set; } = true;

        /// <summary>
        /// Enables Soft Delete (IsDeleted = true) interceptors.
        /// </summary>
        public bool EnableSoftDelete { get; set; } = true;

        /// <summary>
        /// Enables logging of queries that exceed a specific threshold.
        /// </summary>
        public bool EnableSlowQueryLogging { get; set; } = true;

        /// <summary>
        /// Threshold in milliseconds for logging slow queries. Default 500ms.
        /// </summary>
        public int SlowQueryThresholdMilliseconds { get; set; } = 500;
    }
}