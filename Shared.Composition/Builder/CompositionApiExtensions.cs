using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Composition.Options;
using Shared.Health.Builder;
using Shared.Swagger.Builder;
namespace Shared.Composition.Builder
{
    public static class CompositionApiExtensions
    {
        /// <summary>
        /// Configures the HTTP request pipeline based on the enabled Shared features.
        /// Order is enforced here to ensure security and error handling work correctly.
        /// </summary>
        public static WebApplication UseSharedInfrastructure(this WebApplication app)
        {
            // 1. Retrieve the configuration we registered earlier
            var options = app.Services.GetRequiredService<SharedInfrastructureOptions>();

            // 2. Global Error Handling (Must be First)
            // The empty lambda is required by the native middleware, 
            // but our IExceptionHandler implementation handles the logic.
            if (options.ErrorHandling is not null)
                app.UseExceptionHandler(opt => { });

            // 3. OpenAPI / Swagger (Dev Only)
            // We check if the feature is enabled AND if we are in Development
            if (options.OpenApi is not null && app.Environment.IsDevelopment())
                app.UseSharedOpenApi();

            // 4. Security Pipeline
            if (options.Security is not null)
            {
                app.UseHttpsRedirection(); // Standard safety
                app.UseCors("AllowedOrigins"); // The policy name defined in Shared.Security
                app.UseAuthentication();
                app.UseAuthorization();
            }

            // 5. Health Checks
            if (options.Health is not null)
                app.UseSharedHealth();

            // 6. Request Logging (Optional)
            // If you had a middleware for logging requests in Shared.Logging, it would go here.
            // if (options.Logging != null) app.UseSerilogRequestLogging(); 

            return app;
        }
    }
}