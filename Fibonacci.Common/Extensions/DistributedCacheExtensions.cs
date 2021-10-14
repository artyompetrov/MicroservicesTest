using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;


namespace Fibonacci.Common.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static IDistributedCache ToKeyPrefixed(this IDistributedCache distributedCache, string prefix)
        {
            return new KeyPrefixedCacheWrapper(distributedCache, prefix);
        }


        public static async Task<T> GetFromJsonAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken token = default)
            where T : class
        {
            var json = await distributedCache.GetStringAsync(key, token)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(json);
        }

        public static async Task<T> GetFromJsonOrCreateAsync<T>(this IDistributedCache distributedCache, string key,
            CancellationToken token = default)
            where T : class, new()
        {
            var obj = await GetFromJsonAsync<T>(distributedCache, key, token);

            if (obj == null)
            {
                obj = new T();
                await SetAsJsonAsync(distributedCache, key, obj, token);
            }

            return obj;
        }

        public static Task SetAsJsonAsync<T>(this IDistributedCache distributedCache, string key, T value, CancellationToken token = default)
            where T : class
        {
            var json = JsonSerializer.Serialize(value);
            
            return distributedCache.SetStringAsync(key, json, token);
        }

        //TODO: implement GetFromCache methods for structs
    }
}
