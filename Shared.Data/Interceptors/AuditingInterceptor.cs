using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Data.Abstractions;
using System.Diagnostics; // Native .NET namespace

namespace Shared.Data.Interceptors
{
    public sealed class AuditingInterceptor(
           TimeProvider timeProvider,
           ICurrentUserService currentUserService) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            var now = timeProvider.GetUtcNow();

            // 1. Try to get the user from the Web Context (standard)
            var userId = currentUserService.EntraId;

            // 2. NATIVE FALLBACK: If web context is null, check the current Activity (Background Jobs)
            // "enduser.id" is the standard OpenTelemetry semantic convention.
            if (string.IsNullOrEmpty(userId))
            {
                userId = Activity.Current?.GetTagItem("enduser.id") as string ?? "System";
            }

            foreach (var entry in eventData.Context.ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedAt = now;
                    entry.Entity.ModifiedBy = userId;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}