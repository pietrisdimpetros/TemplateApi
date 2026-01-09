using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Data.Abstractions;
namespace Shared.Data.Interceptors
{
    public sealed class SoftDeleteInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            foreach (var entry in eventData.Context.ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    // 1. Cancel the physical delete
                    entry.State = EntityState.Modified;

                    // 2. Set the soft delete flags
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = timeProvider.GetUtcNow();
                }
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}