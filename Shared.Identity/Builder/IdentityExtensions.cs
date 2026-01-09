using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Identity.Data;
using Shared.Identity.Entities;
using Shared.Identity.Options;

namespace Shared.Identity.Builder
{
    public static class IdentityExtensions
    {
        public static IServiceCollection AddSharedIdentity(
            this IServiceCollection services,
            Action<IdentityInfrastructureOptions> configure)
        {
            var options = new IdentityInfrastructureOptions { ConnectionString = "" };
            configure(options);

            // 1. Register the Context with strict Migration Isolation
            services.AddDbContext<AppIdentityDbContext>(dbOptions =>
            {
                dbOptions.UseSqlServer(options.ConnectionString, sql =>
                {
                    // CRITICAL: We tell this context to use a separate History Table 
                    // located inside its own schema.
                    // Result: [identity].[__EFMigrationsHistory]
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", options.SchemaName);
                });

                if (options.EnableDetailedErrors) dbOptions.EnableDetailedErrors();
            });

            // 2. Register Standard Identity
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}