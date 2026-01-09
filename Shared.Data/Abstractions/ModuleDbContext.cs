using Microsoft.EntityFrameworkCore;

namespace Shared.Data.Abstractions
{
    /// <summary>
    /// A base DbContext that enforces Schema-per-Module isolation.
    /// All module-specific contexts must inherit from this.
    /// </summary>
    public abstract class ModuleDbContext(DbContextOptions options) : DbContext(options)
    {
        /// <summary>
        /// Gets the schema name for this module (e.g., "ordering", "catalog").
        /// This creates a strict physical boundary in the database.
        /// </summary>
        protected abstract string Schema { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Enforce the Schema
            // All tables for this context will be created under [Schema].[TableName]
            if (!string.IsNullOrWhiteSpace(Schema))
            {
                modelBuilder.HasDefaultSchema(Schema);
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}