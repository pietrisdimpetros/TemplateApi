using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Shared.Swagger.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Shared.Swagger.Builder
{
    public static class SwaggerApiExtensions
    {
        /// <summary>
        /// Adds Shared.OpenApi infrastructure using Swashbuckle.
        /// Configures Swagger Generation with optional JWT Security.
        /// </summary>
        public static IServiceCollection AddSharedOpenApi(
            this IServiceCollection services,
            Action<OpenApiOptions> configure)
        {
            // 1. Configure Options
            var options = new OpenApiOptions
            {
                DocumentTitle = "API",
                DocumentVersion = "v1"
            };
            configure(options);

            services.AddSingleton(options);

            // 2. Add Endpoints Explorer (Required for Minimal APIs)
            services.AddEndpointsApiExplorer();

            // 3. Add Swashbuckle Generator
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(options.DocumentVersion, new OpenApiInfo { Title = options.DocumentTitle, Version = options.DocumentVersion });

                // Use FullName to avoid conflicts between classes with the same name in different namespaces
                c.CustomSchemaIds(type => type.FullName);

                // Define the Scheme
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                });
                c.SchemaGeneratorOptions = new SchemaGeneratorOptions
                {
                    UseInlineDefinitionsForEnums = true,
                };

                // Add Security Requirement globally
                c.AddSecurityRequirement((document) => new OpenApiSecurityRequirement()
                {
                    [new OpenApiSecuritySchemeReference("oauth2", document)] = []
                });
            });

            return services;
        }
    }
}
