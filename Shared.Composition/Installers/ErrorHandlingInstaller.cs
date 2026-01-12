using Shared.Composition.Options;
using Shared.ErrorHandling.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Composition.Installers
{
    public class ErrorHandlingInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.ErrorHandling is null)
                return;
            services.AddSharedErrorHandling(opt =>
            {
                opt.IncludeStackTrace = rootOptions.ErrorHandling.IncludeStackTrace;
            });
        }
    }
}