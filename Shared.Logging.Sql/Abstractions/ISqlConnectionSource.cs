namespace Shared.Logging.Sql.Abstractions
{
    /// <summary>
    /// Abstract provider to resolve the target database connection string at runtime.
    /// This allows the logger to depend on the ServiceProvider rather than a hardcoded string.
    /// </summary>
    public interface ISqlConnectionSource
    {
        /// <summary>
        /// Retrieves the connection string.
        /// Returns null or throws if no database is available.
        /// </summary>
        Task<string?> GetConnectionStringAsync(CancellationToken cancellationToken);
    }
}