using Microsoft.EntityFrameworkCore;
using Shared.Data.Abstractions;
using TemplateApi.Business.Constants;
using TemplateApi.Business.Entities;

namespace TemplateApi.Business.Data
{
    public class CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : ModuleDbContext(options)
    {
        // 1. Define the Schema strictly
        protected override string Schema => ModuleSchemas.Catalog;

        public DbSet<Product>? Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // The base class handles "HasDefaultSchema(Schema)" automatically!
            base.OnModelCreating(modelBuilder);

            // Module-specific config
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Global Query Filter for Soft Delete (Optional but recommended)
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
        }
    }
}