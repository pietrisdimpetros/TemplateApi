using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.ErrorHandling.Options;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace Shared.ErrorHandling.Handler
{
    internal sealed class GlobalExceptionHandler(
     ILogger<GlobalExceptionHandler> logger,
     ErrorHandlingOptions options) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (logger.IsEnabled(LogLevel.Error))
                // 1. Log the error using the native logger
                logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

            // 2. Determine Status Code and Title based on Exception Type
            var (statusCode, title, detail) = MapExceptionToResponse(exception);

            // 3. Construct ProblemDetails
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Type = $"https://httpstatuses.com/{statusCode}",
                Instance = httpContext.Request.Path
            };

            // 4. Add Extensions (Stack Trace if enabled)
            if (options.IncludeStackTrace)
                problemDetails.Extensions.Add("stackTrace", exception.StackTrace);

            // 5. Write Response
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            // Return true to signal that the exception has been handled
            return true;
        }

        private static (int StatusCode, string Title, string Detail) MapExceptionToResponse(Exception exception)
        {
            return exception switch
            {
                // 404 Not Found
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found", exception.Message),

                // 400 Bad Request (Validation)
                ValidationException => (StatusCodes.Status400BadRequest, "Validation Error", exception.Message),
                ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid Argument", exception.Message),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Argument", exception.Message),

                // 401 Unauthorized
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Access is denied due to invalid credentials."),

                // 403 Forbidden
                InvalidOperationException when exception.Message.Contains("Forbidden") => (StatusCodes.Status403Forbidden, "Forbidden", exception.Message),

                // 408 Request Timeout
                TimeoutException => (StatusCodes.Status408RequestTimeout, "Request Timeout", "The operation timed out."),

                // 501 Not Implemented
                NotImplementedException => (StatusCodes.Status501NotImplemented, "Not Implemented", "This feature is not yet implemented."),

                // 500 Internal Server Error (Default)
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.")
            };
        }
    }
}