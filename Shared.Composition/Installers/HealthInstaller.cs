using Microsoft.Extensions.DependencyInjection;
using Shared.Composition.Options;
using Shared.Health.Builder;

namespace Shared.Composition.Installers
{
    public class HealthInstaller : IInfrastructureInstaller
    {
        /// <summary>
        /// Standard install required by the interface.
        /// Delegates to the specialized overload with no extra checks.
        /// </summary>
        public void Install(IServiceCollection services, SharedInfrastructureOptions options)
        {
            Install(services, options, null);
        }

        /// <summary>
        /// Specialized install that allows injecting external health checks (e.g. from Program.cs).
        /// </summary>
        public void Install(
            IServiceCollection services,
            SharedInfrastructureOptions options,
            Action<IHealthChecksBuilder>? extraHealthChecks)
        {
            if (options.Health is null)
                return;

            // 1. Register base infrastructure (Shared.Health)
            var healthBuilder = services.AddSharedHealth(opt =>
            {
                opt.LivenessEndpoint = options.Health.LivenessEndpoint;
                opt.ReadinessEndpoint = options.Health.ReadinessEndpoint;
                opt.EnableLogPublisher = options.Health.EnableLogPublisher;
                opt.EnableDefaultHealthCheck = options.Health.EnableDefaultHealthCheck;
            });

            // 2. Inject Business Logic Checks
            // This is where 'Program.cs' adds "sql_server", "graph_api", etc.
            extraHealthChecks?.Invoke(healthBuilder);
        }
    }
}