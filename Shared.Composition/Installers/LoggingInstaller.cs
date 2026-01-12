using Shared.Composition.Options;
using Shared.Logging.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Composition.Installers
{
    public class LoggingInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.Logging is null)
                return;
            services.AddSharedLogging(opt =>
            {
                opt.EnableDetailedOutput = rootOptions.Logging.EnableDetailedOutput;
                opt.EnableEnrichment = rootOptions.Logging.EnableEnrichment;
            });
        }
    }
}