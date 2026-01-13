using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Workers.Audit.Services
{
    // Update Primary Constructor to accept workerName
    public abstract class AuditedBackgroundService(ILogger logger, string? workerName = null) : BackgroundService
    {
        // 1. Define the Missing Constant
        public const string ActivitySourceName = "Shared.Workers.Audit";

        // 2. Define the Missing Property
        protected string WorkerName { get; } = workerName ?? "UnknownWorker";

        protected abstract Task ExecuteIterationAsync(CancellationToken stoppingToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                logger.LogInformation("Starting audited background service: {Service}", GetType().Name);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to start background service: {Service}", GetType().Name);
                throw;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ExecuteIterationAsync(stoppingToken);
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
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

                    logger.LogError(ex, "Worker Operation Failed: {@Problem}", problem);
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

        // 3. Define the Missing Helper Method
        protected async Task ExecuteTraceableAsync(string activityName, Func<CancellationToken, Task> action, CancellationToken stoppingToken)
        {
            // Simple wrapper to support the logic in AuditedPeriodicService
            using var activity = new ActivitySource(ActivitySourceName).StartActivity(activityName);
            await action(stoppingToken);
        }
    }
}