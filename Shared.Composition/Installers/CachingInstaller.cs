using Shared.Composition.Options;
using Shared.Caching.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Composition.Installers
{
    public class CachingInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.Caching is null)
                return;
            services.AddSharedCaching(opt =>
            {
                opt.ConnectionString = rootOptions.Caching.ConnectionString;
                opt.InstanceName = rootOptions.Caching.InstanceName;
                opt.DefaultExpirationMinutes = rootOptions.Caching.DefaultExpirationMinutes;
                opt.DataProtection = rootOptions.Caching.DataProtection;
            });
        }
    }
}