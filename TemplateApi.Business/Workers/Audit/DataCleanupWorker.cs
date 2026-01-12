using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Workers.Audit.Services;
using TemplateApi.Business.Data;

namespace TemplateApi.Business.Workers.Audit
{
    public class DataCleanupWorker(
        ILogger<DataCleanupWorker> logger,
        IServiceProvider serviceProvider)
        : AuditedPeriodicService(logger, "DataCleanup") 
    {
        // Schedule: Every 1 hour
        protected override TimeSpan Period => TimeSpan.FromHours(1);

        // Logic
        protected override async Task ExecuteIterationAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            logger.LogInformation("Scanning for obsolete products...");

            // await db.SaveChangesAsync(stoppingToken);

            await Task.CompletedTask;
        }
    }
}