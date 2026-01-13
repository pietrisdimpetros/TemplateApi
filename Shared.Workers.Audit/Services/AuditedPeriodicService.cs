using Microsoft.Extensions.Logging;

namespace Shared.Workers.Audit.Services
{
    public abstract class AuditedPeriodicService(
        ILogger logger,
        string workerName) : AuditedBackgroundService(logger, workerName)
    {
        protected abstract TimeSpan Period { get; }

        // 1. This implements the abstract method from the Base Class
        protected override async Task ExecuteIterationAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Period);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("{Worker} started. Interval: {Period}", WorkerName, Period);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    // Calls the helper we added to the base class
                    await ExecuteTraceableAsync($"{WorkerName}-Tick", ExecuteTickAsync, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("{Worker} stopping.", WorkerName);
            }
        }

        // 2. Define a NEW abstract method for child classes (like DataCleanupWorker) to use
        // Rename this from ExecuteIterationAsync to ExecuteTickAsync to avoid collision
        protected abstract Task ExecuteTickAsync(CancellationToken stoppingToken);
    }
}