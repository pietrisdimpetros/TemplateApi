using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Workers.Audit.Services
{
    // Update Primary Constructor to accept workerName
    public abstract class AuditedBackgroundService(ILogger<AuditedBackgroundService> logger, string? workerName = null) : BackgroundService
    {
        // 1. Define the Missing Constant
        public const string ActivitySourceName = "Shared.Workers.Audit";

        // 2. Define the Missing Property
        protected string WorkerName { get; } = workerName ?? "UnknownWorker";

        protected abstract Task ExecuteIterationAsync(CancellationToken stoppingToken);
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Starting audited background service: {Service}", GetType().Name);

            // 1. Track consecutive errors for Backoff
            int consecutiveErrors = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                // 2. Measure uptime to determine stability
                var uptime = Stopwatch.StartNew();

                try
                {
                    await ExecuteIterationAsync(stoppingToken);
                    // If the iteration method returns normally, we assume the work is done (for non-infinite loops).
                    break;
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                    break;
                }
                catch (Exception ex)
                {
                    uptime.Stop();

                    // 3. Stability Check: If it ran for > 2 minutes, it's not a startup crash. Reset backoff.
                    if (uptime.Elapsed > TimeSpan.FromMinutes(2))
                    {
                        consecutiveErrors = 0;
                    }

                    consecutiveErrors++;

                    // 4. Calculate Exponential Backoff (Base 2s, Max 5m)
                    var delaySeconds = Math.Min(300, Math.Pow(2, consecutiveErrors));
                    // 5. Add Jitter (0-1000ms) to prevent thundering herd
                    var jitter = Random.Shared.Next(0, 1000);
                    var delay = TimeSpan.FromSeconds(delaySeconds).Add(TimeSpan.FromMilliseconds(jitter));

                    var problem = new
                    {
                        Type = ex.GetType().FullName,
                        Title = "Background Worker Failure",
                        Status = 500,
                        Detail = ex.Message,
                        Instance = GetType().Name,
                        RetryCount = consecutiveErrors,
                        NextRetryIn = delay,
                        TraceId = Activity.Current?.TraceId.ToString()
                    };

                    logger.LogError(ex, "Worker Failed. Retrying in {Delay}s. Context: {@Problem}", delay.TotalSeconds, problem);

                    try
                    {
                        await Task.Delay(delay, stoppingToken);
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
        protected static async Task ExecuteTraceableAsync(string activityName, Func<CancellationToken, Task> action, CancellationToken stoppingToken)
        {
            using var activity = new ActivitySource(ActivitySourceName).StartActivity(activityName);
            await action(stoppingToken);
        }
    }
}