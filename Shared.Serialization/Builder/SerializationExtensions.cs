using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Shared.Serialization.Context;
using Shared.Serialization.Options;

namespace Shared.Serialization.Builder
{
    public static class SerializationExtensions
    {
        /// <summary>
        /// Adds Shared.Serialization infrastructure.
        /// Configures Global JSON options (CamelCase, IgnoreNulls) and Source Generation.
        /// </summary>
        public static IServiceCollection AddSharedSerialization(
            this IServiceCollection services,
            Action<SerializationOptions>? configure = null)
        {
            // 1. Setup Options
            var options = new SerializationOptions();
            configure?.Invoke(options);

            // 2. Create the JsonSerializerOptions instance
            // This instance is registered as a Singleton so Caching/Messaging libs can use it.
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = options.NamingPolicy,
                DefaultIgnoreCondition = options.IgnoreCondition,
                WriteIndented = options.WriteIndented,
                TypeInfoResolver = AppJsonContext.Default
            };
            
            // Register additional contexts from the app
            foreach (var resolver in options.TypeInfoResolverChain)
            {
                jsonOptions.TypeInfoResolverChain.Insert(0, resolver);
            }
            
            // Make it read-only to prevent runtime modification
            jsonOptions.MakeReadOnly();

            services.AddSingleton(jsonOptions);
            services.AddSingleton(options);

            // 3. Configure Minimal APIs (Microsoft.AspNetCore.Http.Json.JsonOptions)
            services.Configure<JsonOptions>(httpJson =>
            {
                httpJson.SerializerOptions.PropertyNamingPolicy = options.NamingPolicy;
                httpJson.SerializerOptions.DefaultIgnoreCondition = options.IgnoreCondition;
                httpJson.SerializerOptions.WriteIndented = options.WriteIndented;

                // Add the Shared Context
                httpJson.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
                foreach (var resolver in options.TypeInfoResolverChain)
                {
                    httpJson.SerializerOptions.TypeInfoResolverChain.Insert(0, resolver);
                }
            });

            // 4. Configure Controllers (Microsoft.AspNetCore.Mvc.JsonOptions)
            // We use Configure<T> to avoid direct reference to Mvc dll if not loaded, 
            // though FrameworkReference ensures it's available.
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(mvcJson =>
            {
                mvcJson.JsonSerializerOptions.PropertyNamingPolicy = options.NamingPolicy;
                mvcJson.JsonSerializerOptions.DefaultIgnoreCondition = options.IgnoreCondition;
                mvcJson.JsonSerializerOptions.WriteIndented = options.WriteIndented;

                // Add the Shared Context
                mvcJson.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
            });

            return services;
        }
    }
}
