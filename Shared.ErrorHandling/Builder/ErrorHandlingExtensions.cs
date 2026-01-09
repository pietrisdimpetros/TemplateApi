using Microsoft.Extensions.DependencyInjection;
using Shared.ErrorHandling.Handler;
using Shared.ErrorHandling.Options;
namespace Shared.ErrorHandling.Builder
{
    public static class ErrorHandlingExtensions
    {
        /// <summary>
        /// Adds the Shared.ErrorHandling infrastructure.
        /// Registers the GlobalExceptionHandler and native ProblemDetails services.
        /// </summary>
        public static IServiceCollection AddSharedErrorHandling(
            this IServiceCollection services,
            Action<ErrorHandlingOptions> configure)
        {
            // 1. Configure Options
            var options = new ErrorHandlingOptions
            {
                IncludeStackTrace = false
            };
            configure(options);

            services.AddSingleton(options);

            // 2. Register Native ProblemDetails service
            services.AddProblemDetails();

            // 3. Register Global Exception Handler
            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }
    }
}