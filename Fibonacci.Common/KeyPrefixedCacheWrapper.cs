using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Fibonacci.Common
{
    public class KeyPrefixedCacheWrapper : IDistributedCache
    {
        private readonly IDistributedCache _distributedCache;
        private readonly string _keyPrefix;
        private readonly bool _useCommonPrefix;

        internal KeyPrefixedCacheWrapper(IDistributedCache distributedCache, string keyPrefix, bool useCommonPrefix = true)
        {
            _distributedCache = distributedCache;
            _keyPrefix = keyPrefix;
            _useCommonPrefix = useCommonPrefix;
        }

        /// <summary>
        /// Sets common prefix for KeyPrefixedCacheWrapper among the AppDomain
        /// </summary>
        public static string CommonPrefix { get; set; } = string.Empty;

        private string AddPrefix(string key)
        {
            //TODO: may be a good idea to implement key concatenation using thread-safe StringBuilder

            if (_useCommonPrefix)
            {
                return CommonPrefix + _keyPrefix + key;
            }
            else
            {
                return _keyPrefix + key;

            }
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
