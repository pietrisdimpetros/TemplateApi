using Shared.Composition.Options;
using Shared.WebPerformance.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Composition.Installers
{
    public class WebPerformanceInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.WebPerformance is null)
                return;
            services.AddSharedWebPerformance(opt =>
            {
                opt.EnableOutputCaching = rootOptions.WebPerformance.EnableOutputCaching;
                opt.DefaultExpirationSeconds = rootOptions.WebPerformance.DefaultExpirationSeconds;
                opt.RedisConnectionString = rootOptions.WebPerformance.RedisConnectionString;
            });
        }
    }
}