using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Identity.Entities; // Assuming ApplicationUser is here

namespace Shared.Identity.Data
{
    // Inherits directly from the library, not Shared.Data
    public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        // We need this property to configure the schema in OnModelCreating
        // Ideally passed via constructor or hardcoded if enforcing convention
        internal const string SchemaName = "identity";

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // 1. ISOLATION: Set the schema for all Identity tables
            builder.HasDefaultSchema(SchemaName);

            base.OnModelCreating(builder);

            // 2. Optional: Rename tables to remove "AspNet" prefix
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        }
    }
}