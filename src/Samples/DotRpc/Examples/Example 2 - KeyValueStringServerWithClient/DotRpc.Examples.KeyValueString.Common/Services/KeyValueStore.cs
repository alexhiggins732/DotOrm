using System.Collections.Concurrent;

public class KeyValueStore : IKeyValueStore
{
    static ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);
    public KeyValueStore() { }

    public bool Add(string key, string value)
    {
        var result = _cache.TryAdd(key, value);
        return result;
    }

    public bool AddOrUpdate(string key, string value)
    {
        var result = _cache.AddOrUpdate(key, value, (key, existingValue) => value);
        return result == value;
    }

    public string? Get(string key)
    {
        _cache.TryGetValue(key, out var value);
        return value;
    }

    public string GetOrAdd(string key, string value)
    {
        var result = _cache.GetOrAdd(key, value);
        return result;
    }

    public bool Remove(string key)
    {
        var result = _cache.TryRemove(key, out var value);
        return result;
    }

    public bool Set(string key, string value)
    {
        var result = _cache.TryUpdate(key, value, key);
        return result;
    }
}
