using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Workers.Audit.Services;
using TemplateApi.Business.Constants;
using TemplateApi.Business.Data;

namespace TemplateApi.Business.Workers.Audit
{
    public class DataCleanupWorker(
        IServiceProvider serviceProvider,
        ILogger<DataCleanupWorker> logger) : AuditedBackgroundService(logger)
    {
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(1);
        private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);

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
            var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            var cutoffDate = DateTimeOffset.UtcNow.Subtract(RetentionPeriod);

            const string sql = $"DELETE FROM [{AuditConstants.Schema}].[{AuditConstants.Table}] WHERE [Timestamp] < @p0";

            var deletedRows = await dbContext.Database.ExecuteSqlRawAsync(
                sql,
                [cutoffDate],
                stoppingToken);

            logger.LogInformation("Deleted {Count} rows from {Schema}.{Table}.",
                deletedRows, AuditConstants.Schema, AuditConstants.Table);
        }
    }
}