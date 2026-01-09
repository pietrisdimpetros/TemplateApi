using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Swagger.Options;

namespace Shared.Swagger.Builder
{
    public static class SwaggerApiAppExtensions
    {
        /// <summary>
        /// Activates the Swagger middleware and Swagger UI.
        /// </summary>
        public static WebApplication UseSharedOpenApi(this WebApplication app)
        {
            // Only expose OpenAPI in Development
            if (!app.Environment.IsDevelopment())
            {
                return app;
            }

            var options = app.Services.GetRequiredService<OpenApiOptions>();

            // 1. Serve the generated Swagger JSON
            app.UseSwagger();

            // 2. Serve the Swagger UI
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{options.DocumentVersion}/swagger.json", options.DocumentTitle);
                c.RoutePrefix = "swagger";

                // Optional: Collapse models by default for cleaner UI
                c.DefaultModelsExpandDepth(-1);
            });

            return app;
        }
    }
}