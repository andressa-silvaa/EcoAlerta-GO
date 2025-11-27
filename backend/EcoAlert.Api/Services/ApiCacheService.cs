using System.Collections.Concurrent;
using EcoAlerta.Api.Models;

namespace EcoAlerta.Api.Services;

public interface IApiCacheService
{
    List<Queimada>? Get(string key);
    void Set(string key, List<Queimada> data, TimeSpan? ttl = null);
    void Clear();
}

public class ApiCacheService : IApiCacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(5);

    public List<Queimada>? Get(string key)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow < entry.ExpiresAt)
            {
                return entry.Data;
            }
            _cache.TryRemove(key, out _);
        }
        return null;
    }

    public void Set(string key, List<Queimada> data, TimeSpan? ttl = null)
    {
        var expiresAt = DateTime.UtcNow.Add(ttl ?? _defaultTtl);
        _cache[key] = new CacheEntry(data, expiresAt);
    }

    public void Clear()
    {
        _cache.Clear();
    }

    private record CacheEntry(List<Queimada> Data, DateTime ExpiresAt);
}

