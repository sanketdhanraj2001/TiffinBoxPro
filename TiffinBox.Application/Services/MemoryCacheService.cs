using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<MemoryCacheService> _logger;

        public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            try
            {
                _memoryCache.TryGetValue(key, out T? value);
                return Task.FromResult(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache key: {Key}", key);
                return Task.FromResult(default(T));
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                if (expiry.HasValue)
                    options.AbsoluteExpirationRelativeToNow = expiry;
                else
                    options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                _memoryCache.Set(key, value, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
            }
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            try
            {
                return Task.FromResult(_memoryCache.TryGetValue(key, out _));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence for key: {Key}", key);
                return Task.FromResult(false);
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

        public Task<long> IncrementAsync(string key, long incrementBy = 1)
        {
            throw new NotImplementedException();
        }

        public Task<long> DecrementAsync(string key, long decrementBy = 1)
        {
            throw new NotImplementedException();
        }

        public Task RefreshAsync(string key, TimeSpan? expiry = null)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern)
        {
            return Task.FromResult<IEnumerable<string>>(new List<string>());
        }
    }
}
