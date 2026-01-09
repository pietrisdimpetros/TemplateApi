namespace Shared.Identity.Options
{
    public sealed class IdentityInfrastructureOptions
    {
        public required string ConnectionString { get; set; }
        public string SchemaName { get; set; } = "identity";
        public bool EnableDetailedErrors { get; set; }

        // Resilience Configuration
        public int MaxRetryCount { get; set; } = 3;
        public int MaxRetryDelaySeconds { get; set; } = 30;
        public int CommandTimeoutSeconds { get; set; } = 30;
    }
}