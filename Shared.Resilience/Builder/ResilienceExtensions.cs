using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Shared.Resilience.Options;
namespace Shared.Resilience.Builder
{
    public static class ResilienceExtensions
    {
        /// <summary>
        /// Adds a standard resilience pipeline (Timeout -> Retry -> Circuit Breaker) to the HttpClient.
        /// Uses Microsoft.Extensions.Resilience (Polly V8).
        /// </summary>
        public static IHttpClientBuilder AddStandardResilience(
            this IHttpClientBuilder builder,
            Action<ResilienceOptions>? configure = null)
        {
            // 1. Configure Options
            var options = new ResilienceOptions();
            configure?.Invoke(options);

            // 2. Define the Pipeline
            builder.AddResilienceHandler("StandardPipeline", pipelineBuilder =>
            {
                // A. Overall Timeout (Outer Layer)
                // Cancels the operation if it takes too long overall (including retries).
                pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(options.TotalRequestTimeoutSeconds));

                // B. Retry Strategy
                // Handles transient failures with exponential backoff.
                pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = options.RetryCount,
                    Delay = TimeSpan.FromSeconds(options.RetryDelaySeconds),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });

                // C. Circuit Breaker (Inner Layer)
                // Stops attempts if the downstream service is consistently failing.
                pipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    SamplingDuration = TimeSpan.FromSeconds(options.TotalRequestTimeoutSeconds * 2),
                    FailureRatio = 0.5, // Break if 50% of requests fail
                    MinimumThroughput = options.CircuitBreakerThreshold,
                    BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds)
                });

                // D. Per-Attempt Timeout (Inner-most Layer)
                // Ensures individual attempts don't hang forever.
                pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(5));
            });

            return builder;
        }
    }
}