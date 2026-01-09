namespace Shared.Logging.Options
{
    public sealed class LoggingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to enable indented JSON formatting for local development.
        /// Default is false (single-line JSON).
        /// </summary>
        public required bool EnableDetailedOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include machine and environment context in logs.
        /// </summary>
        public required bool EnableEnrichment { get; set; }
    }
}