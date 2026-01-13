using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;
namespace Shared.Data.Interceptors
{
    //
    public sealed class SlowQueryInterceptor(
          ILogger<SlowQueryInterceptor> logger,
          int thresholdMilliseconds) : DbCommandInterceptor
    {
        // 1. Existing Reader Override (SELECT)
        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            LogIfSlow(command, eventData);
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        // 2. NonQuery Override (UPDATE, DELETE, INSERT)
        public override async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            LogIfSlow(command, eventData);
            return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        // 3. Scalar Override (SELECT COUNT, etc.)
        public override async ValueTask<object?> ScalarExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            object? result,
            CancellationToken cancellationToken = default)
        {
            LogIfSlow(command, eventData);
            return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        // DRY Helper method
        private void LogIfSlow(DbCommand command, CommandExecutedEventData eventData)
        {
            if (eventData.Duration.TotalMilliseconds > thresholdMilliseconds && logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "SLOW QUERY DETECTED ({Duration}ms): {CommandText}",
                    eventData.Duration.TotalMilliseconds,
                    command.CommandText);
            }
        }
    }
}