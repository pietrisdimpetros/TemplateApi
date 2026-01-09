using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace TemplateApi.Buisness.Health.Checks
{
    public class GraphFunctionalityCheck : IHealthCheck
    {
        // Inject your Graph Client or Service here
        // private readonly IGraphClient _graphClient;

        public GraphFunctionalityCheck(/* IGraphClient graphClient */)
        {
            // _graphClient = graphClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Perform a lightweight operation to verify connectivity
                // var user = await _graphClient.Me.Request().GetAsync();

                // If successful:
                return HealthCheckResult.Healthy("Graph API is reachable.");
            }
            catch (Exception ex)
            {
                // If failed:
                return new HealthCheckResult(
                    context.Registration.FailureStatus,
                    "Graph API is unreachable.",
                    ex);
            }
        }
    }
}