using Microsoft.AspNetCore.Mvc;
using Shared.Caching.Services;
using System.Text.Json;
using TemplateApi.Models;

namespace TemplateApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
            logger.LogInformation("Getting weather forecast at {Time}", DateTime.UtcNow);

            // Simulate some logic for Telemetry tracing
            using (var activity = System.Diagnostics.Activity.Current?.Source.StartActivity("CalculateForecast"))
            {
                activity?.AddTag("forecast.count", 5);

                return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
            }
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
