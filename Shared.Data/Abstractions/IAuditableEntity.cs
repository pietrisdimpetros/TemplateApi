namespace Shared.Data.Abstractions
{
    /// <summary>
    /// Entities implementing this will automatically track creation/modification metadata.
    /// </summary>
    public interface IAuditableEntity
    {
        DateTimeOffset CreatedAt { get; set; }
        string? CreatedBy { get; set; }
        DateTimeOffset? ModifiedAt { get; set; }
        string? ModifiedBy { get; set; }
    }
}