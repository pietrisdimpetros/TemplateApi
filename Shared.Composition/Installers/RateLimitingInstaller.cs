using Shared.Composition.Options;
using Shared.RateLimiting.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Composition.Installers
{
    public class RateLimitingInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.RateLimiting is null)
                return;
            services.AddSharedRateLimiting(opt =>
            {
                opt.PolicyName = rootOptions.RateLimiting.PolicyName;
                opt.PermitLimit = rootOptions.RateLimiting.PermitLimit;
                opt.WindowSeconds = rootOptions.RateLimiting.WindowSeconds;
                opt.QueueLimit = rootOptions.RateLimiting.QueueLimit;
            });
        }
    }
}