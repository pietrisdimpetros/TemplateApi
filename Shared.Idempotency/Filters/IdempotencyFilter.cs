using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Idempotency.Attributes;
using Shared.Idempotency.Options;
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
            // 1. Check if the attribute is present
            // We only run this logic if the [Idempotent] attribute is explicitly used.
            var isIdempotent = context.ActionDescriptor.EndpointMetadata
                .Any(m => m is IdempotentAttribute);

            if (!isIdempotent)
            {
                await next();
                return;
            }

            // 2. Check for Header
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

            var cacheKey = $"Idempotency:{idempKey}";

            // 3. CHECK CACHE (The "Short-Circuit")
            var cachedData = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                logger.LogInformation("Idempotency Hit: Returning cached response for Key {Key}", idempKey);

                // Deserialize the saved response
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

            // 4. EXECUTE (The "Real Work")
            var executedContext = await next();

            // 5. CACHE RESULT (If successful)
            if (executedContext.Result is ObjectResult objectResult && objectResult.StatusCode is >= 200 and < 300)
            {
                var model = new IdempotencyModel(objectResult.StatusCode ?? 200, objectResult.Value);

                var serialized = JsonSerializer.Serialize(model);

                await cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.ExpirationMinutes)
                });
            }
        }
    }
}