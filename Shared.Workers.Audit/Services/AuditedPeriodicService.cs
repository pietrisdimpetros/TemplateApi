using Microsoft.Extensions.Logging;

namespace Shared.Workers.Audit.Services
{
    /// <summary>
    /// A standardized host for recurring interval tasks.
    /// </summary>
    public abstract class AuditedPeriodicService(
        ILogger logger,
        string workerName) : AuditedBackgroundService(logger, workerName)
    {
        protected abstract TimeSpan Period { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Period);

            if (logger.IsEnabled(LogLevel.Information))
                logger.LogInformation("{Worker} started. Interval: {Period}", WorkerName, Period);

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await ExecuteTraceableAsync($"{WorkerName}-Tick", ExecuteIterationAsync, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("{Worker} stopping.", WorkerName);
            }
        }

        protected abstract Task ExecuteIterationAsync(CancellationToken stoppingToken);
    }
}