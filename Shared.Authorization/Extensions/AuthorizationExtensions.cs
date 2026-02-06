using Microsoft.Extensions.DependencyInjection;
namespace Shared.Authorization.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddSharedAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization();
            return services;
        }
    }
}