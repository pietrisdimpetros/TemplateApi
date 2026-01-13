using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Logging.Sql.Abstractions;
using Shared.Logging.Sql.Options;

namespace Shared.Logging.Sql.Infrastructure
{
    /// <summary>
    /// Runs at startup to verify database connectivity and provision the Schema/Table.
    /// Throws strict exceptions if the environment is invalid.
    /// </summary>
    internal sealed class SqlLogInitializer(
        ISqlConnectionSource connectionSource,
        IOptions<SqlLoggingOptions> options,
        ILogger<SqlLogInitializer> logger) : IHostedService
    {
        private readonly SqlLoggingOptions _options = options.Value;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Verifying SQL Database injection for Logging...");

            // 1. Resolve Connection String
            var connectionString = await connectionSource.GetConnectionStringAsync(cancellationToken);

            // 2. Strict Validation
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogCritical("SQL Logging is enabled, but no SQL Database connection could be resolved from the Service Provider. Ensure a database module (like Shared.Data) is registered.");
                throw new InvalidOperationException("SQL Logging is enabled, but no SQL Database connection could be resolved from the Service Provider. Ensure a database module (like Shared.Data) is registered.");
            }

            // 3. Provisioning (Ensure Exists)
            try
            {
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync(cancellationToken);

                // Ensure Schema
                var schemaSql = $@"
                    IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{_options.SchemaName}')
                    BEGIN
                        EXEC('CREATE SCHEMA [{_options.SchemaName}]')
                    END";

                using (var cmd = new SqlCommand(schemaSql, conn))
                {
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }

                // Ensure Table
                var tableSql = $@"
                    IF NOT EXISTS (SELECT * FROM sys.objects 
                                   WHERE object_id = OBJECT_ID(N'[{_options.SchemaName}].[{_options.TableName}]') 
                                   AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [{_options.SchemaName}].[{_options.TableName}](
                            [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
                            [Timestamp] DATETIMEOFFSET NOT NULL,
                            [Level] NVARCHAR(20) NOT NULL,
                            [SourceContext] NVARCHAR(256),
                            [Message] NVARCHAR(MAX),
                            [Exception] NVARCHAR(MAX) NULL,
                            [TraceId] NVARCHAR(100) NULL,
                            [SpanId] NVARCHAR(100) NULL,
                            [MachineName] NVARCHAR(100) NULL,
                            INDEX [IX_{_options.TableName}_Timestamp] ([Timestamp] DESC)
                        )
                    END";

                using (var cmd = new SqlCommand(tableSql, conn))
                {
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("SQL Logging Infrastructure Ready. Target: {Schema}.{Table}", _options.SchemaName, _options.TableName);
                }
                
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Failed to provision SQL Logging infrastructure.");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}