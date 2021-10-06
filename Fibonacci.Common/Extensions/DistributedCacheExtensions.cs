using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Fibonacci.Common.Services;
using Microsoft.Extensions.Caching.Distributed;
using 

namespace Fibonacci.Common.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static IDistributedCache ToKeyPrefixed(this IDistributedCache distributedCache, string prefix)
        {
            return new KeyPrefixedCache(distributedCache, prefix);
        }

        public static async T GetDeserializedAsync<T>(this IDistributedCache distributedCache, string prefix)
        {
            return JsonSerializer.DeserializeAsync<T>();

            //TODO: continue here
        }
    }
}
