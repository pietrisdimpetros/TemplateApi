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
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
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
                // A. Update Retry Count
                resilienceOptions.Retry.MaxRetryAttempts = options.MaxRetries;

                // B. Sync Timeouts
                // Total Request Timeout (The hard limit for the whole operation including retries)
                resilienceOptions.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

                // Attempt Timeout (The limit for a single try)
                // We set this slightly lower than Total to allow for retries, 
                // or equal if we want the attempt to take the full time.
                resilienceOptions.AttemptTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

                // C. Circuit Breaker Sampling Fix
                // RULE: SamplingDuration must be >= 2 * AttemptTimeout
                // We set it to 2x the global timeout to be safe.
                resilienceOptions.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(options.TimeoutSeconds * 2);

                // Optional: Adjust failure ratio
                resilienceOptions.CircuitBreaker.FailureRatio = 0.5;
            });

            return services;
        }
    }
}
