using Shared.Composition.Options;
using Shared.FeatureManagement.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Composition.Installers
{
    public class FeatureManagementInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.FeatureManagement is null)
                return;
            services.AddSharedFeatureManagement(opt =>
            {
                opt.FailIfMissing = rootOptions.FeatureManagement.FailIfMissing;
                opt.SectionName = rootOptions.FeatureManagement.SectionName;
            });
        }
    }
}