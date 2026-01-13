using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Logging.Sql.Abstractions;
using Shared.Logging.Sql.Internal;
using Shared.Logging.Sql.Options;
using System.Data;

namespace Shared.Logging.Sql.Services
{
    internal sealed class SqlLogProcessor(
        LogBuffer buffer,
        ISqlConnectionSource connectionSource,
        IOptions<SqlLoggingOptions> options,
        ILogger<SqlLogProcessor> logger) : BackgroundService
    {
        private readonly SqlLoggingOptions _options = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var batch = new List<LogEntry>(_options.BatchSize);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ReadAndProcessBatchAsync(batch, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error flushing logs to SQL. Retrying in 5 seconds...");
                    try { await Task.Delay(5000, stoppingToken); } catch { /* Ignore cancellation */ }
                }
            }
        }

        private async Task ReadAndProcessBatchAsync(List<LogEntry> batch, CancellationToken ct)
        {
            // 1. Read from Channel
            await foreach (var log in buffer.ReadAllAsync(ct))
            {
                batch.Add(log);
                if (batch.Count >= _options.BatchSize) break;
            }

            if (batch.Count == 0) return;

            // 2. Bulk Insert
            await BulkInsertAsync(batch, ct);
            batch.Clear();

            // 3. Wait (if we didn't fill a batch, wait before reading again to save CPU)
            if (batch.Count < _options.BatchSize)
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                linkedCts.CancelAfter(TimeSpan.FromSeconds(_options.FlushIntervalSeconds));
                try { await Task.Delay(Timeout.Infinite, linkedCts.Token); } catch (OperationCanceledException) { }
            }
        }

        private async Task BulkInsertAsync(List<LogEntry> logs, CancellationToken ct)
        {
            // Resolve connection string fresh every time (handles rotation/dynamic changes)
            var connectionString = await connectionSource.GetConnectionStringAsync(ct);

            if (string.IsNullOrWhiteSpace(connectionString)) return;

            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);

            using var transaction = conn.BeginTransaction();

            var sql = $@"
                INSERT INTO [{_options.SchemaName}].[{_options.TableName}] 
                (Timestamp, Level, SourceContext, Message, Exception, TraceId, SpanId, MachineName)
                VALUES (@ts, @lvl, @src, @msg, @ex, @trc, @spn, @mac)";

            foreach (var log in logs)
            {
                using var cmd = new SqlCommand(sql, conn, transaction);

                cmd.Parameters.Add("@ts", SqlDbType.DateTimeOffset).Value = log.Timestamp;
                cmd.Parameters.Add("@lvl", SqlDbType.NVarChar, 20).Value = log.Level;
                cmd.Parameters.Add("@src", SqlDbType.NVarChar, 256).Value = log.Category ?? "";
                cmd.Parameters.Add("@msg", SqlDbType.NVarChar, -1).Value = log.Message;
                cmd.Parameters.Add("@ex", SqlDbType.NVarChar, -1).Value = (object?)log.Exception ?? DBNull.Value;
                cmd.Parameters.Add("@trc", SqlDbType.NVarChar, 100).Value = (object?)log.TraceId ?? DBNull.Value;
                cmd.Parameters.Add("@spn", SqlDbType.NVarChar, 100).Value = (object?)log.SpanId ?? DBNull.Value;
                cmd.Parameters.Add("@mac", SqlDbType.NVarChar, 100).Value = (object?)log.MachineName ?? DBNull.Value;

                await cmd.ExecuteNonQueryAsync(ct);
            }

            await transaction.CommitAsync(ct);
        }
    }
}