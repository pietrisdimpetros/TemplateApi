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
            var connectionString = await connectionSource.GetConnectionStringAsync(ct);
            if (string.IsNullOrWhiteSpace(connectionString)) return;

            // 1. Prepare Data in Memory (Zero Network IO)
            // using DataTable is efficient enough for batch sizes < 10k
            using var table = new DataTable();
            table.Columns.Add("Timestamp", typeof(DateTimeOffset));
            table.Columns.Add("Level", typeof(string));
            table.Columns.Add("SourceContext", typeof(string));
            table.Columns.Add("Message", typeof(string));
            table.Columns.Add("Exception", typeof(string));
            table.Columns.Add("TraceId", typeof(string));
            table.Columns.Add("SpanId", typeof(string));
            table.Columns.Add("MachineName", typeof(string));

            foreach (var log in logs)
            {
                table.Rows.Add(
                    log.Timestamp,
                    log.Level,
                    log.Category ?? string.Empty,
                    log.Message,
                    log.Exception, // Assumed string or null
                    log.TraceId,
                    log.SpanId,
                    log.MachineName
                );
            }

            // 2. Stream to SQL Server (Single Network Call)
            // UseInternalTransaction: More efficient than external SqlTransaction for bulk copies
            using var bulk = new SqlBulkCopy(connectionString, SqlBulkCopyOptions.TableLock | SqlBulkCopyOptions.UseInternalTransaction);

            bulk.DestinationTableName = $"[{_options.SchemaName}].[{_options.TableName}]";
            bulk.BulkCopyTimeout = 60; // Increase timeout for large batches

            // Explicit Mappings ensure safety against column reordering
            foreach (DataColumn col in table.Columns)
            {
                bulk.ColumnMappings.Add(col.ColumnName, col.ColumnName);
            }

            try
            {
                await bulk.WriteToServerAsync(table, ct);
            }
            catch (Exception ex)
            {
                // Fallback or specific error handling (e.g., dropping bad batch vs retry)
                logger.LogError(ex, "Bulk insert failed for {Count} logs.", logs.Count);
                throw; // Let the outer loop retry logic handle the pause
            }
        }
    }
}