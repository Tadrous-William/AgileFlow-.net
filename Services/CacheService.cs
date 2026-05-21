using AgileTaskManager.Services.Interfaces;

namespace AgileTaskManager.Services;

public class CacheService : ICacheService
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly Dictionary<string, int> _accessCounts = new();

    public async Task<T?> GetAsync<T>(string key)
    {
        await Task.CompletedTask;
        
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.Expiry.HasValue && DateTime.UtcNow > entry.Expiry.Value)
            {
                _cache.Remove(key);
                return default;
            }

            _accessCounts[key] = _accessCounts.GetValueOrDefault(key, 0) + 1;
            return (T?)entry.Value;
        }

        return default;
    }

    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> factory)
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        await SetAsync(key, value);
        return value;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        await Task.CompletedTask;
        
        _cache[key] = new CacheEntry
        {
            Value = value,
            Expiry = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : null,
            Priority = CachePriority.Normal
        };
    }

    public async Task RemoveAsync(string key)
    {
        await Task.CompletedTask;
        _cache.Remove(key);
        _accessCounts.Remove(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        await Task.CompletedTask;
        
        var keysToRemove = _cache.Keys
            .Where(k => System.Text.RegularExpressions.Regex.IsMatch(k, pattern))
            .ToList();

        foreach (var key in keysToRemove)
        {
            _cache.Remove(key);
            _accessCounts.Remove(key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        await Task.CompletedTask;
        
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.Expiry.HasValue && DateTime.UtcNow > entry.Expiry.Value)
            {
                _cache.Remove(key);
                return false;
            }
            return true;
        }
        return false;
    }

    public async Task RefreshAsync(string key)
    {
        await Task.CompletedTask;
        
        if (_cache.TryGetValue(key, out var entry))
        {
            entry.Expiry = DateTime.UtcNow.AddHours(1);
        }
    }

    public async Task ClearAsync()
    {
        await Task.CompletedTask;
        _cache.Clear();
        _accessCounts.Clear();
    }

    public async Task<TimeSpan?> GetExpiryAsync<T>(string key)
    {
        await Task.CompletedTask;
        
        if (_cache.TryGetValue(key, out var entry))
        {
            return entry.Expiry.HasValue 
                ? entry.Expiry.Value - DateTime.UtcNow 
                : null;
        }
        return null;
    }

    public async Task SetWithPriorityAsync<T>(string key, T value, CachePriority priority, TimeSpan? expiry = null)
    {
        await Task.CompletedTask;
        
        _cache[key] = new CacheEntry
        {
            Value = value,
            Expiry = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : null,
            Priority = priority
        };
    }

    public async Task<T[]> GetMultipleAsync<T>(string[] keys)
    {
        await Task.CompletedTask;
        
        var results = new List<T>();
        foreach (var key in keys)
        {
            var value = await GetAsync<T>(key);
            if (value != null)
                results.Add(value);
        }
        return results.ToArray();
    }

    public async Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null)
    {
        await Task.CompletedTask;
        
        foreach (var item in items)
        {
            await SetAsync(item.Key, item.Value, expiry);
        }
    }

    public async Task<long> GetSizeAsync()
    {
        await Task.CompletedTask;
        return _cache.Count;
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        await Task.CompletedTask;
        
        var hitCount = _accessCounts.Values.Sum();
        var missCount = _cache.Count * 10 - hitCount; // Simplified calculation

        return new CacheStatistics
        {
            TotalItems = _cache.Count,
            MemoryUsage = _cache.Count * 1024, // Simplified estimation
            HitCount = hitCount,
            MissCount = Math.Max(0, missCount)
        };
    }

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime? Expiry { get; set; }
        public CachePriority Priority { get; set; }
    }
}
