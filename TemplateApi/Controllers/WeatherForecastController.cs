using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using Shared.Caching.Services;
using System.Text.Json;
using TemplateApi.Models;
using TemplateApi.Business.Features;
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
            var now = DateTime.UtcNow;

            // 1. Write to Cache
            logger.LogInformation("Writing to cache...");
            await cache.SetAsync(key, new { Message = "Hello Redis", Timestamp = now }, TimeSpan.FromMinutes(1));

            // 2. Read from Cache
            logger.LogInformation("Reading from cache...");
            var cachedValue = await cache.GetAsync<JsonElement>(key);

            return Ok(new
            {
                Status = "Success",
                Source = "Redis",
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
    }
}
