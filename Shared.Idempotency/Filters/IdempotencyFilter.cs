using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Idempotency.Attributes;
using Shared.Idempotency.Options;
using System.Security.Claims;
using System.Text.Json;

namespace Shared.Idempotency.Filters
{
    public sealed class IdempotencyFilter(
        IDistributedCache cache,
        IOptions<IdempotencyOptions> options,
        ILogger<IdempotencyFilter> logger) : IAsyncActionFilter
    {
        private readonly IdempotencyOptions _options = options.Value;
        private record IdempotencyModel(int StatusCode, object? Body);

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // ---------------------------------------------------------------------------
            // 1. PRE-CHECKS
            // ---------------------------------------------------------------------------
            var isIdempotent = context.ActionDescriptor.EndpointMetadata
                .Any(m => m is IdempotentAttribute);

            if (!isIdempotent)
            {
                await next();
                return;
            }

            // Reject anonymous users. Idempotency requires a stable User Identity.
            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedObjectResult(new { Error = "Idempotency is only supported for authenticated users." });
                return;
            }

            // Check for Header
            if (!context.HttpContext.Request.Headers.TryGetValue(_options.HeaderName, out var idempKey) || string.IsNullOrWhiteSpace(idempKey))
            {
                if (_options.EnforceHeader)
                {
                    context.Result = new BadRequestObjectResult(new { Error = $"Missing required header: {_options.HeaderName}" });
                    return;
                }
                await next();
                return;
            }

            // Resolve User ID
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? context.HttpContext.User.Identity?.Name
                         ?? "anonymous";

            // ---------------------------------------------------------------------------
            // 2. CACHE KEYS
            // ---------------------------------------------------------------------------
            // Primary Key: Stores the final result
            var cacheKey = $"Idempotency:{userId}:{idempKey}";

            // Lock Key: Indicates a request is currently IN FLIGHT
            var lockKey = $"{cacheKey}:processing";

            // ---------------------------------------------------------------------------
            // 3. CHECK EXISTING RESULT (Happy Path)
            // ---------------------------------------------------------------------------
            var cachedData = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Idempotency Hit: Returning cached response for Key {Key} (User: {User})", idempKey, userId);
                }

                var responseModel = JsonSerializer.Deserialize<IdempotencyModel>(cachedData);
                if (responseModel is not null)
                {
                    context.Result = new ObjectResult(responseModel.Body)
                    {
                        StatusCode = responseModel.StatusCode
                    };
                    return;
                }
            }

            // ---------------------------------------------------------------------------
            // 4. CHECK & ACQUIRE LOCK (Concurrency Protection)
            // ---------------------------------------------------------------------------
            var isProcessing = !string.IsNullOrEmpty(await cache.GetStringAsync(lockKey));
            if (isProcessing)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Idempotency Conflict: Request {Key} is already being processed.", idempKey!);
                }
                context.Result = new ConflictObjectResult(new
                {
                    Error = "A request with this Idempotency-Key is currently being processed.",
                    Code = "IDEMPOTENCY_CONFLICT"
                });
                return;
            }

            // Set the "Processing" lock.
            // TTL: Short (e.g., 2 minutes). Just enough to cover the execution time.
            // Prevents deadlocks if the server crashes mid-request.
            await cache.SetStringAsync(lockKey, DateTime.UtcNow.ToString("O"), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            });

            try
            {
                // ---------------------------------------------------------------------------
                // 5. EXECUTE (The "Real Work")
                // ---------------------------------------------------------------------------
                var executedContext = await next();

                // ---------------------------------------------------------------------------
                // 6. CACHE RESULT (If successful)
                // ---------------------------------------------------------------------------
                // Only cache successful or business-logic failures (2xx, 4xx). 
                // Do not cache 500s, so the client can retry safely.
                if (executedContext.Result is ObjectResult objectResult &&
                    objectResult.StatusCode is >= 200 and < 500) // Updated to include 4xx
                {
                    var model = new IdempotencyModel(objectResult.StatusCode ?? 200, objectResult.Value);
                    var serialized = JsonSerializer.Serialize(model);

                    await cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.ExpirationMinutes)
                    });
                }
            }
            finally
            {
                // ---------------------------------------------------------------------------
                // 7. RELEASE LOCK
                // ---------------------------------------------------------------------------
                // Always remove the processing lock, even if the action failed.
                await cache.RemoveAsync(lockKey);
            }
        }
    }
}