using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Workers.Audit.Services
{
    public abstract class AuditedBackgroundService(ILogger logger) : BackgroundService
    {
        protected abstract Task ExecuteIterationAsync(CancellationToken stoppingToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1. Startup Safety: If the service fails to even start (e.g. bad DI), we log critical.
            try
            {
                logger.LogInformation("Starting audited background service: {Service}", GetType().Name);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to start background service: {Service}", GetType().Name);
                throw; // Startup failures are fatal, let the host know.
            }

            // 2. The Resilience Loop
            // This ensures that if the worker crashes (e.g. DB disconnect), it restarts.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Run the actual worker logic
                    await ExecuteIterationAsync(stoppingToken);

                    // Note: If ExecuteIterationAsync returns cleanly (without cancellation), 
                    // it means the worker finished its job naturally. 
                    // We break the loop to stop the service gracefully.
                    break;
                }
                catch (OperationCanceledException)
                {
                    // Normal shutdown signal
                    break;
                }
                catch (Exception ex)
                {
                    // 3. Standardized Error Handling (The "ProblemDetails" for Workers)
                    // We structure this object so it appears cleanly in your SQL Logs
                    var problem = new
                    {
                        Type = ex.GetType().FullName,
                        Title = "Background Worker Failure",
                        Status = 500,
                        Detail = ex.Message,
                        Instance = GetType().Name,
                        Timestamp = DateTimeOffset.UtcNow,
                        Machine = Environment.MachineName,
                        TraceId = Activity.Current?.TraceId.ToString()
                    };

                    // Log as Error. This flows to SqlLogProcessor -> Database
                    logger.LogError(ex, "Worker Operation Failed: {@Problem}", problem);

                    // 4. Circuit Breaker / Cool-down
                    // Pause execution to prevent CPU/DB spamming during outages
                    logger.LogWarning("Pausing service {Service} for 1 minute due to error.", GetType().Name);

                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }

            logger.LogInformation("Audited background service stopped: {Service}", GetType().Name);
        }
    }
}