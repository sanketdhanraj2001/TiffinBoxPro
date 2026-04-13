using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<Dictionary<string, T?>> GetMultipleAsync<T>(IEnumerable<string> keys);
        Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null);
        Task RemoveMultipleAsync(IEnumerable<string> keys);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
        Task<long> IncrementAsync(string key, long incrementBy = 1);
        Task<long> DecrementAsync(string key, long decrementBy = 1);
        Task RefreshAsync(string key, TimeSpan? expiry = null);
        Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern);
    }
}
