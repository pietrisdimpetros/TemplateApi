using Microsoft.Extensions.Logging;

namespace Shared.Workers.Audit.Services
{
    /// <summary>
    /// A standardized host for recurring interval tasks (Cron-like behavior) that require auditing.
    /// </summary>
    public abstract class AuditedPeriodicService(ILogger logger) : AuditedBackgroundService(logger)
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
                    // The "Template Method" pattern ensures we never run without an Audit Context
                    await ExecuteTraceableAsync($"{WorkerName}-Tick", ExecuteIterationAsync, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation("{Worker} stopping.", WorkerName);
            }
        }

        /// <summary>
        /// The unit of work to perform on every tick.
        /// </summary>
        protected abstract Task ExecuteIterationAsync(CancellationToken stoppingToken);
    }
}