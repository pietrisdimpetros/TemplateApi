namespace Shared.Caching.Options
{
    /// <summary>
    /// Specific settings for enabling Data Protection within the Caching module.
    /// Renamed to avoid conflict with Microsoft.AspNetCore.DataProtection.DataProtectionOptions.
    /// </summary>
    public sealed class CachingDataProtectionOptions
    {
        /// <summary>
        /// Gets or sets the enabled state.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the unique Application Name.
        /// Critical for isolating keys between different apps sharing the same Redis.
        /// </summary>
        public required string ApplicationName { get; set; }

        /// <summary>
        /// Optional: Override the connection string specifically for Data Protection.
        /// If null, uses the parent CachingOptions.ConnectionString.
        /// </summary>
        public string? ConnectionStringOverride { get; set; }
    }
}