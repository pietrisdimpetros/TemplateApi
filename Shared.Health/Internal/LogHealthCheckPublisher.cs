using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Health.Internal
{
    /// <summary>
    /// A simple publisher that logs the health report to the standard ILogger.
    /// Optimized to reduce noise by only logging transitions or failures.
    /// </summary>
    internal sealed class LogHealthCheckPublisher(ILogger<LogHealthCheckPublisher> logger) : IHealthCheckPublisher
    {
        // Track the previous status to avoid spamming "Healthy" logs every 30s
        private HealthStatus _prevStatus = HealthStatus.Unhealthy;
        // Define a simple source name for this internal diagnostic
        private static readonly ActivitySource ActivitySource = new("Shared.Health.Publisher");

        public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
        {
            // Start a native Activity Scope
            // The Interceptor in Shared.Data will automatically "see" this tag via Activity.Current
            using var activity = ActivitySource.StartActivity("HealthCheckPublisher", ActivityKind.Internal);
            activity?.AddTag("enduser.id", "Health-Probe-Service");
            var status = report.Status;

            // 1. Log Changes in Overall Status
            if (status != _prevStatus)
            {
                var level = status == HealthStatus.Healthy ? LogLevel.Information : LogLevel.Error;
                if (logger.IsEnabled(level))
                {
                    logger.Log(level, "Health Status Change: {PrevStatus} -> {NewStatus}. Duration: {Duration}",
                        _prevStatus,
                        status,
                        report.TotalDuration);
                }

                _prevStatus = status;
            }
            else if (status != HealthStatus.Healthy && logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Health Status remains {Status}. Duration: {Duration}",
                    status,
                    report.TotalDuration);
            }

            // 2. Log specific unhealthy entries (Always visible for debugging issues)
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