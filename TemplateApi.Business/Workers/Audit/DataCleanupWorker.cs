using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Workers.Audit.Services;
using TemplateApi.Business.Data;

namespace TemplateApi.Business.Workers.Audit
{
    public class DataCleanupWorker(
        ILogger<DataCleanupWorker> logger,
        IServiceProvider serviceProvider)
        : AuditedPeriodicService(logger)
    {
        // 1. Identity: "Worker-DataCleanup"
        protected override string WorkerName => "DataCleanup";

        // 2. Schedule: Every 1 hour
        protected override TimeSpan Period => TimeSpan.FromHours(1);

        // 3. Logic
        protected override async Task ExecuteIterationAsync(CancellationToken stoppingToken)
        {
            // Create a scope because CatalogDbContext is Scoped, but Worker is Singleton
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            // Simulate cleanup logic
            // Any SQL executed here will automatically have CreatedBy = "Worker-DataCleanup"
            logger.LogInformation("Scanning for obsolete products...");

            // await db.SaveChangesAsync(stoppingToken);

            await Task.CompletedTask;
        }
    }
}