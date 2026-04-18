using SmartInventorySystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartInventorySystem.Infrastructure.Services
{
    public class NullCacheService : ICacheService
    {
        public Task<T?> GetAsync<T>(string key)
        {
            return Task.FromResult(default(T));
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefix)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(false);
        }
    }
}
