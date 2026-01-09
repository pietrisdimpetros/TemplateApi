using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Data.Abstractions;

namespace Shared.Data.Interceptors
{
    public sealed class AuditingInterceptor(
           TimeProvider timeProvider,
           ICurrentUserService currentUserService) : SaveChangesInterceptor // Inject the service
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            var now = timeProvider.GetUtcNow();
            // 1. Retrieve the User ID (EntraId) from the service
            var userId = currentUserService.EntraId ?? "System";

            foreach (var entry in eventData.Context.ChangeTracker.Entries<IAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    // 2. Set the Creator
                    entry.Entity.CreatedBy = userId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedAt = now;
                    // 3. Set the Modifier
                    entry.Entity.ModifiedBy = userId;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}