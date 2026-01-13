using Shared.Composition.Options;
using Shared.Identity.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Composition.Installers
{
    public class IdentityInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.Identity is null)
                return;

            services.AddSharedIdentity(opt =>
            {
                opt.ConnectionString = rootOptions.Identity.ConnectionString;
                opt.SchemaName = rootOptions.Identity.SchemaName;

                opt.MaxRetryCount = rootOptions.Identity.MaxRetryCount;
                opt.MaxRetryDelaySeconds = rootOptions.Identity.MaxRetryDelaySeconds;
                opt.CommandTimeoutSeconds = rootOptions.Identity.CommandTimeoutSeconds;
                opt.EnableDetailedErrors = rootOptions.Identity.EnableDetailedErrors;
            });
        }
    }
}