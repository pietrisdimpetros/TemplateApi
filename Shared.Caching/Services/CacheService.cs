using Microsoft.Extensions.Caching.Distributed;
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
        /// Retrieves a value from the cache and deserializes it to the specified type.
        /// Returns default(T) if the key is not found.
        /// </summary>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Serializes and saves a value to the cache with an optional expiration.
        /// If expiration is null, the default configured expiration is used.
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes the specified key from the cache.
        /// </summary>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Default implementation of ICacheService using IDistributedCache and System.Text.Json.
    /// </summary>
    internal sealed class CacheService(
        IDistributedCache cache,
        IOptions<CachingOptions> options,
        JsonSerializerOptions jsonOptions) : ICacheService
    {
        private readonly JsonSerializerOptions _jsonOptions = jsonOptions;
        private readonly CachingOptions _options = options.Value;
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var bytes = await cache.GetAsync(key, cancellationToken);
            if (bytes is null || bytes.Length == 0)
                return default;

            return JsonSerializer.Deserialize<T>(bytes, _jsonOptions);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);

            var entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes)
            };

            await cache.SetAsync(key, bytes, entryOptions, cancellationToken);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
    }
}