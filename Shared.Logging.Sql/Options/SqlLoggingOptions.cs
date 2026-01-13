namespace Shared.Logging.Sql.Options
{
    public sealed class SqlLoggingOptions
    {
        // Note: ConnectionString is deliberately absent here. 
        // It is resolved dynamically via ISqlConnectionSource.

        /// <summary>
        /// The schema name to isolate logs (e.g., "logging").
        /// Defaults to "logging".
        /// </summary>
        public string SchemaName { get; set; } = "logging";

        /// <summary>
        /// The table name for log storage.
        /// Defaults to "SystemLogs".
        /// </summary>
        public string TableName { get; set; } = "SystemLogs";

        /// <summary>
        /// Batch size for bulk insertion.
        /// </summary>
        public int BatchSize { get; set; } = 100;

        /// <summary>
        /// Time to wait before flushing the buffer if batch size isn't met.
        /// </summary>
        public int FlushIntervalSeconds { get; set; } = 5;
    }
}