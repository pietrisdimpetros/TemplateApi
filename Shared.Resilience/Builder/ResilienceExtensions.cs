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

                var attemptTimeout = CalculateAttemptTimeout(options);
                // 3. Circuit Breaker (Inner Layer)
                pipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    // Consistency: Sample for 2x the attempt duration
                    SamplingDuration = attemptTimeout * 2,

                    FailureRatio = 0.5,
                    MinimumThroughput = options.CircuitBreakerThreshold,
                    BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerBreakDurationSeconds)
                });

                // 4. Per-Attempt Timeout (Inner-most Layer)
                pipelineBuilder.AddTimeout(attemptTimeout);
            });

            return builder;
        }
        private static TimeSpan CalculateAttemptTimeout(ResilienceOptions options)
        {
            var totalSlots = options.RetryCount + 1;
            var safeSeconds = (options.TotalRequestTimeoutSeconds / (double)totalSlots) * 0.7;
            return TimeSpan.FromSeconds(Math.Max(2.0, safeSeconds));
        }
    }
}