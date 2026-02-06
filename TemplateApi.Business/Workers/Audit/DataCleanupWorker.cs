using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Workers.Audit.Services; // Ensure the entity namespace is imported if different
using TemplateApi.Business.Data;

namespace TemplateApi.Business.Workers.Audit
{
    public class DataCleanupWorker(
        IServiceProvider serviceProvider,
        ILogger<DataCleanupWorker> logger) : AuditedBackgroundService(logger)
    {
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);
        private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);
        private const int BatchSize = 5000;

        protected override async Task ExecuteIterationAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(CleanupInterval);
            await PerformCleanupAsync(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await PerformCleanupAsync(stoppingToken);
            }
        }

        private async Task PerformCleanupAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting Audit Cleanup...");

            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

            var cutoffDate = DateTimeOffset.UtcNow.Subtract(RetentionPeriod);
            int rowsAffected;
            int totalDeleted = 0;

            do
            {
                // FIX: Use modern ExecuteDeleteAsync instead of raw SQL.
                // This is safer, faster, and database agnostic (works on Postgres/SQLServer etc).
                // Note: Assumes 'AuditLogs' is the DbSet property name in AuditDbContext.
                rowsAffected = await dbContext.Logs
                    .Where(x => x.Timestamp < cutoffDate)
                    .Take(BatchSize)
                    .ExecuteDeleteAsync(stoppingToken);

                totalDeleted += rowsAffected;

                if (rowsAffected > 0 && logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Deleted batch of {Count} rows...", rowsAffected);
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }

            } while (rowsAffected > 0 && !stoppingToken.IsCancellationRequested);

            if (totalDeleted > 0 && logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("Audit Cleanup completed. Total deleted: {Count} rows.", totalDeleted);
        }
    }
}