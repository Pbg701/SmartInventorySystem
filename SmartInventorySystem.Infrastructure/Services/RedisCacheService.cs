using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SmartInventorySystem.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace SmartInventorySystem.Infrastructure.Services
{

    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(data))
                    return default;

                return JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
                };

                var data = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, data, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache for key: {Key}", key);
            }
        }

        public async Task RemoveByPrefixAsync(string prefix)
        {
            // Note: Redis doesn't support pattern-based removal directly
            // This would require SCAN command which is not available in IDistributedCache
            // For production, consider using StackExchange.Redis directly
            _logger.LogWarning("RemoveByPrefixAsync not fully implemented for Redis");
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var data = await _cache.GetStringAsync(key);
                return data != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
