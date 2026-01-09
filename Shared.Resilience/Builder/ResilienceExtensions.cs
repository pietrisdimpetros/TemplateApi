using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Shared.Resilience.Options;

namespace Shared.Resilience.Builder
{
    public static class ResilienceExtensions
    {
        public static IHttpClientBuilder AddStandardResilience(
            this IHttpClientBuilder builder,
            Action<ResilienceOptions>? configure = null)
        {
            var options = new ResilienceOptions();
            configure?.Invoke(options);

            builder.AddResilienceHandler("StandardPipeline", pipelineBuilder =>
            {
                // 1. Overall Timeout (Outer Layer)
                pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(options.TotalRequestTimeoutSeconds));

                // 2. Retry Strategy
                pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = options.RetryCount,
                    Delay = TimeSpan.FromSeconds(options.RetryDelaySeconds),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });

                // 3. Dynamic Attempt Timeout Calculation (The Fix)
                // Logic: Ensure the attempt timeout fits within the Total Timeout allowing for retries + backoff.
                var totalSlots = options.RetryCount + 1;
                var safeAttemptTimeout = (options.TotalRequestTimeoutSeconds / (double)totalSlots) * 0.7;
                var finalAttemptTimeout = Math.Max(2.0, safeAttemptTimeout);

                // 4. Circuit Breaker (Inner Layer)
                pipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    // Consistency: Sample for 2x the attempt duration
                    SamplingDuration = TimeSpan.FromSeconds(finalAttemptTimeout * 2),

                    FailureRatio = 0.5,
                    MinimumThroughput = options.CircuitBreakerThreshold,
                    BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds)
                });

                // 5. Per-Attempt Timeout (Inner-most Layer)
                pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(finalAttemptTimeout));
            });

            return builder;
        }
    }
}