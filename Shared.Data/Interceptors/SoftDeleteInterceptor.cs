using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Data.Abstractions;

namespace Shared.Data.Interceptors
{
    public sealed class SoftDeleteInterceptor(
        TimeProvider timeProvider,
        ICurrentUserService currentUserService) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            // 2. Retrieve the current user (consistent with AuditingInterceptor)
            var userId = currentUserService.EntraId ?? "System";

            foreach (var entry in eventData.Context.ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    // Cancel the physical delete
                    entry.State = EntityState.Modified;

                    // Set the soft delete flags
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = timeProvider.GetUtcNow();

                    // 3. Populate DeletedBy
                    entry.Entity.DeletedBy = userId;
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}