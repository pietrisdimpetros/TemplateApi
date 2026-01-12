using Microsoft.Extensions.DependencyInjection;
using Shared.Composition.Options;
using Shared.Swagger.Builder;

namespace Shared.Composition.Installers
{
    public class OpenApiInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.OpenApi is null)
                return;
            services.AddSharedOpenApi(opt =>
            {
                opt.DocumentTitle = rootOptions.OpenApi.DocumentTitle;
                opt.DocumentVersion = rootOptions.OpenApi.DocumentVersion;
                opt.EnableAuth = rootOptions.OpenApi.EnableAuth;
            });
        }
    }
}