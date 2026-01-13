using Microsoft.EntityFrameworkCore;
using Shared.Data.Abstractions;
using TemplateApi.Business.Constants;
using Shared.Logging.Sql.Internal;
namespace TemplateApi.Business.Data
{
    // A lightweight, read-only context for your Admin Dashboard
    public class AuditDbContext(DbContextOptions<AuditDbContext> options) : ModuleDbContext(options)
    {
        protected override string Schema => AuditConstants.Schema;

        public DbSet<LogEntry> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ENFORCEMENT: Maps the entity to [audit].[Logs]
            modelBuilder.Entity<LogEntry>(entity =>
            {
                entity.ToTable(AuditConstants.Table, AuditConstants.Schema);

                // Optimized for Read-Only access
                entity.HasNoKey(); // Or map the Id if you need pagination
                entity.Property(e => e.Message).HasColumnName("Message");
                entity.Property(e => e.Timestamp).HasColumnName("Timestamp");
                entity.Property(e => e.Level).HasColumnName("Level");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}