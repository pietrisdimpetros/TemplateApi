using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Shared.Security.Options;
namespace Shared.Security.Builder
{
    public static class SecurityExtensions
    {
        /// <summary>
        /// Adds Shared.Security infrastructure.
        /// Configures JWT Bearer Authentication and CORS policies.
        /// </summary>
        public static IServiceCollection AddSharedSecurity(
            this IServiceCollection services,
            Action<SecurityOptions> configure)
        {
            // 1. Configure Options
            var options = new SecurityOptions
            {
                Authority = string.Empty,
                Audience = string.Empty,
                AllowedOrigins = []
            };
            configure(options);

            services.AddSingleton(options);
            // 2. Configure Authentication (JWT)
            services
                .AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtOptions =>
                {
                    jwtOptions.MapInboundClaims = false;
                    jwtOptions.Authority = options.Authority;
                    jwtOptions.Audience = options.Audience;
                    jwtOptions.RequireHttpsMetadata = true; // Strict HTTPS

                    jwtOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.FromSeconds(30) // Reduce default drift tolerance
                    };
                });

            // 3. Configure Authorization
            // Registers default authorization services required by middleware.
            services.AddAuthorization();

            // 4. Configure CORS
            services.AddCors(corsOptions =>
            {
                corsOptions.AddPolicy("AllowedOrigins", policy =>
                {
                    policy.WithOrigins(options.AllowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Often required for JWTs in browsers
                });

                // Set as default policy to simplify middleware usage
                corsOptions.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(options.AllowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            return services;
        }
    }
}