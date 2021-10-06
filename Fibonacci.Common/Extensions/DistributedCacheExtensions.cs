using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Fibonacci.Common.Services;
using Microsoft.Extensions.Caching.Distributed;


namespace Fibonacci.Common.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static IDistributedCache ToKeyPrefixed(this IDistributedCache distributedCache, string prefix)
        {
            return new KeyPrefixedCache(distributedCache, prefix);
        }

        //TODO: check that CancellationToken is used correctly
        public static async Task<T> GetFromJsonAsync<T>(this IDistributedCache distributedCache, string key, CancellationToken token = default)
            where T : class
        {
            var json = await distributedCache.GetStringAsync(key, token);

            if (string.IsNullOrEmpty(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(json);
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
