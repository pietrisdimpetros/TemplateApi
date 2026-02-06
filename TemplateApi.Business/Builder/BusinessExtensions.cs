using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using TemplateApi.Business.Authorization;
namespace TemplateApi.Business.Builder
{
    public static class BusinessExtensions
    {
        public static IServiceCollection AddBusinessLogic(this IServiceCollection services)
        {
            // ---------------------------------------------------------
            // Authorization Handlers (Explicit Registration)
            // ---------------------------------------------------------
            // We register handlers explicitly. No Reflection scanning.
            // If you rename a class, this breaks at compile time (Good).

            services.AddScoped<IAuthorizationHandler, ProductAuthorizationHandler>();

            // Add future handlers here:
            // services.AddScoped<IAuthorizationHandler, OrderAuthorizationHandler>();

            return services;
        }
    }
}