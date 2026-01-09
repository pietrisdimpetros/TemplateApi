using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace TemplateApi.Buisness.Health.Checks
{
    public class DemoResult
    {
        public bool IsValid { get; set; }
    }   
    /// <summary>
    /// A template for functional health checks against internal services.
    /// </summary>
    public sealed class DemoCheck() : IHealthCheck
    {
        private const string CheckName = "Graph API Functional Check";

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Execute the actual BL logic
              
                var result = new DemoResult
                {
                    IsValid = true
                };

                // 2. Validate the state (Healthy)
                if (result is { IsValid: true })
                {
                    return HealthCheckResult.Healthy($"{CheckName} passed.");
                }

                // 3. Handle a non-critical failure (Degraded)
                // The service is running, but the data returned is unexpected.
                return HealthCheckResult.Degraded($"{CheckName} returned invalid data structure.");
            }
            catch (OperationCanceledException)
            {
                // 4. Handle Timeouts (Degraded)
                // Keeps the pod alive but signals latency issues to the Log Publisher.
                return HealthCheckResult.Degraded($"{CheckName} timed out.");
            }
            catch (Exception ex)
            {
                // 5. Critical Failure (Unhealthy)
                // This triggers the Error log in LogHealthCheckPublisher.
                return HealthCheckResult.Unhealthy($"{CheckName} failed with an exception.", ex);
            }
        }
    }
}