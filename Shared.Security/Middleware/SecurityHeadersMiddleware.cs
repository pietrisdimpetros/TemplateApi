using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
namespace Shared.Security.Middleware
{
    public sealed class SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment env)
    {
        public Task InvokeAsync(HttpContext context)
        {
            var headers = context.Response.Headers;

            // 1. Anti-MIME Sniffing
            headers.TryAdd("X-Content-Type-Options", "nosniff");

            // 2. Anti-Clickjacking
            headers.TryAdd("X-Frame-Options", "DENY");

            // 3. XSS Protection
            headers.TryAdd("X-XSS-Protection", "1; mode=block");

            // 4. Strict Transport Security (HSTS) - Force HTTPS
            headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

            // 5. Content Security Policy (CSP)
            if (env.IsDevelopment())
            {
                // Development: Allow Swagger UI (Inline scripts, styles, and data images)
                headers.TryAdd("Content-Security-Policy",
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " + // Needed for Swagger UI logic
                    "style-src 'self' 'unsafe-inline'; " +                // Needed for Swagger UI themes
                    "img-src 'self' data: https:; " +                     // Needed for Swagger logos
                    "connect-src 'self' https:; " +                       // Allow calling the API itself
                    "object-src 'none'; " +
                    "frame-ancestors 'none'; " +
                    "upgrade-insecure-requests;");
            }
            else
            {
                // Production: Strict defaults
                headers.TryAdd("Content-Security-Policy",
                    "default-src 'self'; " +
                    "img-src 'self' data: https:; " +
                    "object-src 'none'; " +
                    "frame-ancestors 'none'; " +
                    "upgrade-insecure-requests;");
            }

            // 6. Referrer Policy
            headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");

            return next(context);
        }
    }
}