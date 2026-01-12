using Shared.Composition.Options;
using Shared.Security.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Composition.Installers
{
    public class SecurityInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.Security is null)
                return;
            services.AddSharedSecurity(opt =>
            {
                opt.Authority = rootOptions.Security.Authority;
                opt.Audience = rootOptions.Security.Audience;
                opt.AllowedOrigins = rootOptions.Security.AllowedOrigins;
            });
        }
    }
}