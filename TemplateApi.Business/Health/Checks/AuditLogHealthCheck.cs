using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Shared.Data.Options;
using TemplateApi.Business.Constants;

namespace TemplateApi.Business.Health.Checks
{
    public class AuditLogHealthCheck(IOptions<DatabaseOptions> options) : IHealthCheck
    {
        private readonly string _connectionString = options.Value.ConnectionString;

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync(ct);

                using var cmd = conn.CreateCommand();

                // ENFORCEMENT: Proves that [audit].[Logs] is actually accessible.
                // We select TOP 0 to test connectivity/permissions without reading data.
                cmd.CommandText = $"SELECT TOP 0 1 FROM [{AuditConstants.Schema}].[{AuditConstants.Table}]";

                await cmd.ExecuteNonQueryAsync(ct);

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Degraded($"Audit Log Table [{AuditConstants.Schema}].[{AuditConstants.Table}] is unreachable.", ex);
            }
        }
    }
}