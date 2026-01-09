using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
namespace Shared.Health.Internal
{
    /// <summary>
    /// A simple publisher that logs the health report to the standard ILogger.
    /// This ensures health status is visible in whatever logging sink (Console, OTel, etc.) is configured.
    /// </summary>
    internal sealed class LogHealthCheckPublisher(ILogger<LogHealthCheckPublisher> logger) : IHealthCheckPublisher
    {
        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            var level = report.Status == HealthStatus.Healthy ? LogLevel.Information : LogLevel.Error;
            if (logger.IsEnabled(level))
                logger.Log(level, "Health Report: {Status}. Total Duration: {Duration}. Entries: {EntryCount}",
                    report.Status,
                    report.TotalDuration,
                    report.Entries.Count);

            foreach (var entry in report.Entries)
            {
                if (entry.Value.Status != HealthStatus.Healthy && logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Unhealthy Check: {Key} - {Description} ({Status})",
                        entry.Key,
                        entry.Value.Description,
                        entry.Value.Status);
                }
            }

            return Task.CompletedTask;
        }
    }
}
