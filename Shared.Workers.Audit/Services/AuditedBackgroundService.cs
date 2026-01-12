using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Shared.Workers.Audit.Services
{
    /// <summary>
    /// Extends BackgroundService to enforce an Audit Context.
    /// This ensures that any data operations performed by the worker are correctly attributed
    /// in the 'CreatedBy'/'ModifiedBy' fields, rather than appearing as 'System'.
    /// </summary>
    public abstract class AuditedBackgroundService(ILogger logger) : BackgroundService
    {
        public const string ActivitySourceName = "Shared.Workers.Audit";
        // Global ActivitySource for all worker auditing
        protected static readonly ActivitySource ActivitySource = new(ActivitySourceName);

        /// <summary>
        /// The identity of this worker (e.g., "EmailSender").
        /// Resulting Audit User: "Worker-EmailSender".
        /// </summary>
        protected abstract string WorkerName { get; }

        /// <summary>
        /// Executes the provided workload inside a traceable Activity Scope.
        /// </summary>
        protected async Task ExecuteTraceableAsync(string operationName, Func<CancellationToken, Task> workload, CancellationToken stoppingToken)
        {
            // 1. Start the Context Bubble
            // This Activity flows via AsyncLocal, so any EF Core Interceptor downstream can see it.
            using var activity = ActivitySource.StartActivity(operationName);

            // 2. Set the Identity Tag (The "Key" to Native Auditing)
            var userId = $"Worker-{WorkerName}";
            activity?.SetTag("enduser.id", userId);

            // 3. Log with Scope for debugging clarity
            using (logger.BeginScope(new Dictionary<string, object> { ["Worker"] = WorkerName, ["User"] = userId }))
            {
                try
                {
                    logger.LogInformation("Starting traceable operation: {Operation}", operationName);
                    await workload(stoppingToken);
                    logger.LogInformation("Completed traceable operation: {Operation}", operationName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed traceable operation: {Operation}", operationName);
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    throw; // Let the Host handle the crash policy
                }
            }
        }
    }
}