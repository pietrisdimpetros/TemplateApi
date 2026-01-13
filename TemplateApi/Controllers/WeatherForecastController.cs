using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Shared.Caching.Services;
using Shared.Idempotency.Attributes;
using System.Text.Json;
using TemplateApi.Business.Features;
using TemplateApi.Models;
namespace TemplateApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableRateLimiting("Standard")]
    [OutputCache(Duration = 10)] // Caches this specific endpoint for 10 seconds
    //[OutputCache(NoStore = true)] // Explicitly disable caching for this endpoint
    public class WeatherForecastController(
     ILogger<WeatherForecastController> logger,
     ICacheService cache, // Injected from Shared.Caching
     IHttpClientFactory clientFactory // Injected from Shared.Networking
     ) : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        /// <summary>
        /// Verifies Logging, Telemetry, and Serialization.
        /// </summary>
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            // Simulate some logic for Telemetry tracing
            using (var activity = System.Diagnostics.Activity.Current?.Source.StartActivity("CalculateForecast"))
            {
                activity?.AddTag("forecast.count", 5);

                return [.. Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })];
            }
        }
        // Requires BOTH "NewGraphCheck" AND "BetaDashboard" to be true
        [HttpGet("advanced-dashboard")]
        [FeatureGate(CurrentFeature.NewGraphCheck, CurrentFeature.BetaDashboard)]
        public IActionResult GetAdvancedDashboard()
        {
            return Ok("Power user access granted.");
        }

        // Requires EITHER "BetaDashboard" OR "PreviewAccess"
        [HttpGet("preview")]
        [FeatureGate(RequirementType.Any, CurrentFeature.BetaDashboard, CurrentFeature.NewGraphCheck)]
        public IActionResult GetPreview()
        {
            return Ok("You have preview access.");
        }

        [HttpGet("graph-check")]
        [FeatureGate(CurrentFeature.NewGraphCheck)] // Requires "NewGraphCheck": true
        public IActionResult GetGraphCheck()
        {
            return Ok("You have access.");
        }
        /// <summary>
        /// Verifies Shared.Caching (Redis) connectivity.
        /// </summary>
        [HttpGet("cache-test")]
        public async Task<IActionResult> TestCache()
        {
            const string key = "test-key";

            // ONE STEP: "Get this key. If you don't have it, run this function to make it."
            var cachedValue = await cache.GetOrCreateAsync(
                key,
                factory: async (ct) =>
                {
                    // logic to create data (e.g. DB query)
                    // This ONLY runs if the cache is empty!
                    logger.LogInformation("Cache miss. Generating new value...");
                    return new { Message = "Hello HybridCache", Timestamp = DateTime.UtcNow };
                },
                expiration: TimeSpan.FromMinutes(1),
                cancellationToken: HttpContext.RequestAborted
            );

            return Ok(new
            {
                Status = "Success",
                Source = "HybridCache",
                RetrievedData = cachedValue
            });
        }
        /// <summary>
        /// Verifies Shared.Networking (Resilience, Header Propagation).
        /// Calls an external URL using the "ResilientClient".
        /// </summary>
        [HttpGet("external-test")]
        public async Task<IActionResult> TestNetworking()
        {
            // 1. Create the named client configured in Shared.Networking
            // This client has Retry (3x), Circuit Breaker, and Timeout (15s) policies attached.
            var client = clientFactory.CreateClient("ResilientClient");

            // 2. Execute Request (Using a robust public API for demo: Google)
            try
            {
                var response = await client.GetAsync("https://www.google.com");

                return Ok(new
                {
                    Status = response.StatusCode,
                    IsSuccess = response.IsSuccessStatusCode,
                    Headers = response.Headers.Select(h => new { h.Key, Value = string.Join(",", h.Value) })
                });
            }
            catch (Exception ex)
            {
                // If the circuit breaker is open or retries fail, this catches it.
                logger.LogError(ex, "External request failed");
                return StatusCode(502, new { Error = "External dependency failed", Details = ex.Message });
            }
        }

        [HttpPost("create-forecast")]
        [Idempotent] // <--- Prevents double-creation
        public IActionResult CreateForecast([FromBody] WeatherForecast forecast)
        {
            var json = JsonSerializer.Serialize(forecast);
            // If client sends "Idempotency-Key: 123", this runs ONCE.
            // The second time they send "123", they get the cached 200 OK immediately.
            return Ok(new { Id = Guid.CreateVersion7(), Status = "Created" , json});
        }
    }
}
