using Microsoft.Extensions.DependencyInjection;
using Shared.Networking.Options;
using System.Net;
using System.Net.Http.Headers;
namespace Shared.Networking.Builder
{
    public static class NetworkingExtensions
    {
        private const string ClientName = "ResilientClient";

        public static IServiceCollection AddSharedNetworking(
            this IServiceCollection services,
            Action<NetworkingOptions> configure)
        {
            var options = new NetworkingOptions();
            configure(options);
            services.AddSingleton(options);

            // Header Propagation
            services.AddHeaderPropagation(o =>
            {
                o.Headers.Add("traceparent");
                o.Headers.Add("tracestate");
            });

            // 1. Configure the Client
            var clientBuilder = services.AddHttpClient(ClientName, client =>
            {
                client.Timeout = Timeout.InfiniteTimeSpan;
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            });

            // 2. Configure Primary Handler (SocketsHttpHandler)
            clientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new SocketsHttpHandler
                {
                    PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                    AutomaticDecompression = DecompressionMethods.All,
                };

                if (options.IgnoreSslErrors)
                {
                    handler.SslOptions.RemoteCertificateValidationCallback =
                        (sender, cert, chain, sslPolicyErrors) => true;
                }

                return handler;
            });

            // 3. Header Propagation
            clientBuilder.AddHeaderPropagation();

            // 4. Standard Resilience (The FIX is below)
            clientBuilder.AddStandardResilienceHandler(resilienceOptions =>
            {
                // 1. Configure Total Timeout (The hard limit)
                resilienceOptions.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

                // 2. Configure Retry Strategy
                resilienceOptions.Retry.MaxRetryAttempts = options.MaxRetries;
                // BackoffType is usually Exponential, so we can't just divide strictly. 
                // But we can ensure the *Individual Attempt* is small enough to fit.

                // 3. Dynamic Attempt Timeout Calculation
                // Logic: If we have 3 retries (4 total attempts), the attempt timeout 
                // must be significantly smaller than Total / 4 to account for backoff delays.

                // Calculate the theoretical maximum slots (Retries + 1 initial attempt)
                var totalSlots = options.MaxRetries + 1;

                // Safety buffer factor (e.g., 0.7) to leave room for the Backoff delays between tries
                var safeAttemptTimeout = (options.TimeoutSeconds / (double)totalSlots) * 0.7;

                // Enforce a sensible minimum (e.g., never less than 2 seconds)
                var finalAttemptTimeout = Math.Max(2.0, safeAttemptTimeout);

                resilienceOptions.AttemptTimeout.Timeout = TimeSpan.FromSeconds(finalAttemptTimeout);

                // 4. Circuit Breaker Sampling
                // Ensure we sample for at least 2x the attempt duration to catch failures accurately
                resilienceOptions.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(finalAttemptTimeout * 2);
            });

            return services;
        }
    }
}
