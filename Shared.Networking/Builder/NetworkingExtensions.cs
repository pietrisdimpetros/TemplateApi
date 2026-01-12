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
            Action<NetworkingOptions> configure,
            Action<IHttpClientBuilder>? builderConfigure = null)
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

            // 4. Standard Resilience
            builderConfigure?.Invoke(clientBuilder);
            return services;
        }
    }
}