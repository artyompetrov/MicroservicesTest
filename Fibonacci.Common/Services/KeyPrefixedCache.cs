using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Fibonacci.Common.Services
{
    class KeyPrefixedCache : IDistributedCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly string _keyPrefix;

        public KeyPrefixedCache(IDistributedCache distributedCache, string keyPrefix)
        {
            _distributedCache = distributedCache;
            _keyPrefix = keyPrefix;
        }

        private string AddPrefix(string key)
        {
            return _keyPrefix + key;
        }

        public byte[] Get(string key) => _distributedCache.Get(AddPrefix(key));

        public Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken()) =>
            _distributedCache.GetAsync(AddPrefix(key), token);

        public void Refresh(string key) => _distributedCache.Refresh(AddPrefix(key));

        public Task RefreshAsync(string key, CancellationToken token = new CancellationToken()) =>
            _distributedCache.RefreshAsync(AddPrefix(key), token);

        public void Remove(string key) => _distributedCache.Remove(AddPrefix(key));

        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken()) =>
            _distributedCache.RemoveAsync(AddPrefix(key), token);

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            _distributedCache.Set(AddPrefix(key), value, options);

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken()) =>
            _distributedCache.SetAsync(AddPrefix(key), value, options, token);
    }
}
