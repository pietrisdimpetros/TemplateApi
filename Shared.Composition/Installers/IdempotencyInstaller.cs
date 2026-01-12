using Microsoft.Extensions.DependencyInjection;
using Shared.Composition.Options;
using Shared.Idempotency.Builder;

namespace Shared.Composition.Installers
{
    public class IdempotencyInstaller : IInfrastructureInstaller
    {
        public void Install(IServiceCollection services, SharedInfrastructureOptions rootOptions)
        {
            if (rootOptions.Idempotency is null)
                return;
            services.AddSharedIdempotency(opt =>
            {
                opt.HeaderName = rootOptions.Idempotency.HeaderName;
                opt.ExpirationMinutes = rootOptions.Idempotency.ExpirationMinutes;
            });
        }
    }
}