namespace Shared.Identity.Options
{
    public sealed class IdentityInfrastructureOptions
    {
        public required string ConnectionString { get; set; }
        public string SchemaName { get; set; } = "identity";
        public bool EnableDetailedErrors { get; set; }
    }
}