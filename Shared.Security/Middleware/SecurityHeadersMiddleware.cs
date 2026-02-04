using Microsoft.AspNetCore.Http;
namespace Shared.Security.Middleware
{
    public sealed class SecurityHeadersMiddleware(RequestDelegate next)
    {
        public Task InvokeAsync(HttpContext context)
        {
            var headers = context.Response.Headers;

            // 1. Anti-MIME Sniffing
            // Prevents browsers from "guessing" the content type if it differs from the declared one.
            headers.TryAdd("X-Content-Type-Options", "nosniff");

            // 2. Anti-Clickjacking
            // Prevents this site from being embedded in an iframe (protects against UI redress attacks).
            headers.TryAdd("X-Frame-Options", "DENY");

            // 3. XSS Protection (Legacy browsers)
            headers.TryAdd("X-XSS-Protection", "1; mode=block");

            // 4. Strict Transport Security (HSTS) - Force HTTPS
            // MaxAge = 1 year, include subdomains.
            headers.TryAdd("Strict-Transport-Security", "max-age=31536000; includeSubDomains");

            // 5. Content Security Policy (CSP)
            // This is a strict starting point. It disables inline scripts/styles.
            // Adjust 'script-src' if you need inline scripts (e.g., Swagger UI issues).
            headers.TryAdd("Content-Security-Policy",
                "default-src 'self'; " +
                "img-src 'self' data: https:; " +
                "object-src 'none'; " +
                "frame-ancestors 'none'; " +
                "upgrade-insecure-requests;");

            // 6. Referrer Policy
            // Only send the origin (domain) when navigating to external sites, not the full URL path.
            headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");

            return next(context);
        }
    }
}