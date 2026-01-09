using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Data.Common;
namespace Shared.Data.Interceptors
{
    public sealed class SlowQueryInterceptor(
          ILogger<SlowQueryInterceptor> logger,
          int thresholdMilliseconds) : DbCommandInterceptor
    {
        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Duration.TotalMilliseconds > thresholdMilliseconds && logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(
                    "SLOW QUERY DETECTED ({Duration}ms): {CommandText}",
                    eventData.Duration.TotalMilliseconds,
                    command.CommandText);
            }

            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }
    }
}