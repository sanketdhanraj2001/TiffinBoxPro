using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiffinBox.Application.Common.Interfaces;

namespace TiffinBox.Application.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(
            IDistributedCache distributedCache,
            ILogger<RedisCacheService> logger)
        {
            _distributedCache = distributedCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await _distributedCache.GetStringAsync(key);
                if (string.IsNullOrEmpty(data))
                    return default;
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions();
                if (expiry.HasValue)
                    options.AbsoluteExpirationRelativeToNow = expiry;
                else
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                var data = JsonConvert.SerializeObject(value);
                await _distributedCache.SetStringAsync(key, data, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var data = await _distributedCache.GetStringAsync(key);
                return !string.IsNullOrEmpty(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence for key: {Key}", key);
                return false;
            }
        }

        public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys)
        {
            var result = new Dictionary<string, T?>();
            foreach (var key in keys)
            {
                var value = await GetAsync<T>(key);
                result[key] = value;
            }
            return result;
        }

        public async Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null)
        {
            foreach (var item in items)
            {
                await SetAsync(item.Key, item.Value, expiry);
            }
        }

        public async Task RemoveMultipleAsync(IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                await RemoveAsync(key);
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
                return cachedValue;

            var freshValue = await factory();
            await SetAsync(key, freshValue, expiry);
            return freshValue;
        }

        public async Task<long> IncrementAsync(string key, long incrementBy = 1)
        {
            try
            {
                var current = await GetAsync<long>(key);
                var newValue = current + incrementBy;
                await SetAsync(key, newValue);
                return newValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing key: {Key}", key);
                return 0;
            }
        }

        public async Task<long> DecrementAsync(string key, long decrementBy = 1)
        {
            try
            {
                var current = await GetAsync<long>(key);
                var newValue = current - decrementBy;
                await SetAsync(key, newValue);
                return newValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrementing key: {Key}", key);
                return 0;
            }
        }

        public async Task RefreshAsync(string key, TimeSpan? expiry = null)
        {
            var value = await GetAsync<string>(key);
            if (value != null)
            {
                await SetAsync(key, value, expiry);
            }
        }

        public async Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern)
        {
            return new List<string>();
        }
    }
}
