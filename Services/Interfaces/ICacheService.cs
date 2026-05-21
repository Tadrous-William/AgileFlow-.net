using System;
using System.Threading.Tasks;

namespace AgileTaskManager.Services.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task<T?> GetAsync<T>(string key, Func<Task<T>> factory);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
    Task RefreshAsync(string key);
    Task ClearAsync();
    Task<TimeSpan?> GetExpiryAsync<T>(string key);
    Task SetWithPriorityAsync<T>(string key, T value, CachePriority priority, TimeSpan? expiry = null);
    Task<T[]> GetMultipleAsync<T>(string[] keys);
    Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null);
    Task<long> GetSizeAsync();
    Task<CacheStatistics> GetStatisticsAsync();
}

public enum CachePriority
{
    Low,
    Normal,
    High
}

public class CacheStatistics
{
    public long TotalItems { get; set; }
    public long MemoryUsage { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public double HitRate => HitCount + MissCount > 0 ? (double)HitCount / (HitCount + MissCount) : 0;
}
