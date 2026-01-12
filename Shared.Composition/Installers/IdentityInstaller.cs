using Shared.Composition.Options;
using Shared.Identity.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Composition.Installers
{
    public class IdentityInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.Logging is null)
                return;
            services.AddSharedIdentity(opt =>
            {
                opt.ConnectionString = rootOptions?.Database?.ConnectionString??throw new Exception("Database connection string is not configured.");
                opt.EnableDetailedErrors = rootOptions.Database.EnableDetailedErrors;
                opt.SchemaName = "identity";
                opt.MaxRetryCount = rootOptions.Database.MaxRetryCount;
                opt.MaxRetryDelaySeconds = rootOptions.Database.MaxRetryDelaySeconds;
                opt.CommandTimeoutSeconds = rootOptions.Database.CommandTimeoutSeconds;
            });
        }
    }
}