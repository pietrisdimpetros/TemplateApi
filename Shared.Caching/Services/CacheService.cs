using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Shared.Caching.Options;
using System.Text.Json;

namespace Shared.Caching.Services
{
    /// <summary>
    /// Defines a strongly-typed contract for caching operations, handling serialization internally.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Removes a value from the cache.
        /// </summary>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an item if it exists, or creates it using the factory if missing.
        /// Uses HybridCache's stampede protection.
        /// </summary>
        Task<T?> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Default implementation of ICacheService using IDistributedCache and System.Text.Json.
    /// </summary>
    internal sealed class CacheService(
        HybridCache hybridCache,
        IOptions<CachingOptions> options,
        JsonSerializerOptions jsonOptions) : ICacheService
    {
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions;
        private readonly CachingOptions _options = options.Value;
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await hybridCache.RemoveAsync(key, cancellationToken);
        }

        public async Task<T?> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var options = expiration.HasValue
                ? new HybridCacheEntryOptions { Expiration = expiration.Value }
                : null;

            return await hybridCache.GetOrCreateAsync(key, factory, options, tags: null, cancellationToken: cancellationToken);
        }

    }
}