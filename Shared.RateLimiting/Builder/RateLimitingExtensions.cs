using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Shared.RateLimiting.Options;
using System.Threading.RateLimiting;

namespace Shared.RateLimiting.Builder
{
    public static class RateLimitingExtensions
    {
        /// <summary>
        /// Adds Shared.RateLimiting infrastructure using the native Microsoft Middleware.
        /// Configures a default Fixed Window limiter.
        /// </summary>
        public static IServiceCollection AddSharedRateLimiting(
            this IServiceCollection services,
            Action<RateLimitingOptions> configure)
        {
            // 1. Configure Options
            var options = new RateLimitingOptions();
            configure(options);
            services.AddSingleton(options);

            // 2. Configure Native Rate Limiter
            services.AddRateLimiter(limiterOptions =>
            {
                // Ensure we return 429 instead of the default 503
                limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Define the Global/Standard Policy
                limiterOptions.AddFixedWindowLimiter(policyName: options.PolicyName, fixedWindow =>
                {
                    fixedWindow.PermitLimit = options.PermitLimit;
                    fixedWindow.Window = TimeSpan.FromSeconds(options.WindowSeconds);
                    fixedWindow.QueueLimit = options.QueueLimit;
                    fixedWindow.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
            });

            return services;
        }

        /// <summary>
        /// Activates the RateLimiting middleware.
        /// </summary>
        public static WebApplication UseSharedRateLimiting(this WebApplication app)
        {
            app.UseRateLimiter();
            return app;
        }
    }
}