using Microsoft.Extensions.DependencyInjection;
using Shared.Composition.Options;

namespace Shared.Composition.Installers
{
    public interface IInfrastructureInstaller
    {
        void Install(IServiceCollection services, SharedInfrastructureOptions options);
    }
}