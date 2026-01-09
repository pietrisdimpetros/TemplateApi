namespace Shared.Data.Abstractions
{
    /// <summary>
    /// Entities implementing this will be "Soft Deleted" (marked inactive) instead of removed.
    /// </summary>
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTimeOffset? DeletedAt { get; set; }
        string? DeletedBy { get; set; }
    }
}