using Microsoft.Extensions.DependencyInjection;
using Shared.Authorization.Extensions;
using Shared.Composition.Options;

namespace Shared.Composition.Installers
{
    public sealed class AuthorizationInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions options)
        {
            services.AddSharedAuthorization();
        }
    }
}