using Shared.Composition.Options;
using Shared.Telemetry.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Workers.Audit.Services;

namespace Shared.Composition.Installers
{
    public class TelemetryInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.Telemetry is null)
                return;
            services.AddSharedTelemetry(opt =>
            {
                opt.ServiceName = rootOptions.Telemetry.ServiceName;
                opt.ServiceVersion = rootOptions.Telemetry.ServiceVersion;
                opt.UseAzureMonitor = rootOptions.Telemetry.UseAzureMonitor;
                opt.OtlpEndpoint = rootOptions.Telemetry.OtlpEndpoint;
                opt.ActivitySources.Add(AuditedBackgroundService.ActivitySourceName);
            });
        }
}
}