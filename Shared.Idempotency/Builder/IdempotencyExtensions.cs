using Microsoft.Extensions.DependencyInjection;
using Shared.Idempotency.Filters;
using Shared.Idempotency.Options;

namespace Shared.Idempotency.Builder
{
    public static class IdempotencyExtensions
    {
        public static IServiceCollection AddSharedIdempotency(
            this IServiceCollection services,
            Action<IdempotencyOptions> configure)
        {
            var options = new IdempotencyOptions();
            configure(options);
            services.AddSingleton(options);

            // Register the filter globally or specifically. 
            // Here we register it as a Scoped service so it can be used by [ServiceFilter] 
            // OR added to the global MVC filters list in Composition.
            // We'll choose Global Registration in Composition to make the Attribute work automatically.
            services.AddScoped<IdempotencyFilter>();

            return services;
        }
    }
}