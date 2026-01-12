using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Composition.Options;
using Shared.Composition.Services;
using Shared.Data.Abstractions;
using Shared.Idempotency.Filters;

namespace Shared.Composition.Installers
{
    public class EssentialsInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions options)
        {
            // 1. HttpContext & User Identity
            services.AddHttpContextAccessor();
            services.TryAddSingleton<ICurrentUserService, WebCurrentUserService>();

            // 2. MVC & Global Filters
            services.AddControllers(mvcOptions =>
            {
                // Global Filter Registration
                // If Idempotency is enabled, we apply the filter globally here
                // (or strictly rely on the Attribute, depending on your preference).
                if (options.Idempotency is not null)
                    mvcOptions.Filters.Add<IdempotencyFilter>();
            });
        }
    }
}
