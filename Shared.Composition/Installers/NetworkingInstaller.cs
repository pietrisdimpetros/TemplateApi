using Shared.Composition.Options;
using Shared.Networking.Builder;
using Shared.Resilience.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Composition.Installers
{
    public class NetworkingInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.Networking is null)
                return;
            services.AddSharedNetworking(
                    // 1. Configure Basic Options
                    opt =>
                    {
                        opt.UserAgent = rootOptions.Networking.UserAgent;
                        opt.TimeoutSeconds = rootOptions.Networking.TimeoutSeconds;
                        opt.MaxRetries = rootOptions.Networking.MaxRetries;
                        opt.IgnoreSslErrors = rootOptions.Networking.IgnoreSslErrors;
                    },
                   // 2. The Hook: Inject the Smart Resilience Logic Here!
                   builder =>
                   {
                       // This uses your Custom Extension from Shared.Resilience
                       builder.AddStandardResilience(resilienceOptions =>
                       {
                           resilienceOptions.RetryCount = rootOptions.Networking.MaxRetries;
                           resilienceOptions.RetryDelaySeconds = rootOptions.Resilience!.RetryDelaySeconds;
                           resilienceOptions.CircuitBreakerBreakDurationSeconds = rootOptions.Resilience!.CircuitBreakerBreakDurationSeconds;
                           resilienceOptions.TotalRequestTimeoutSeconds = rootOptions.Networking.TimeoutSeconds;
                           resilienceOptions.CircuitBreakerThreshold = rootOptions.Resilience!.CircuitBreakerThreshold;
                       });
                   }
                );
        }
    }
}