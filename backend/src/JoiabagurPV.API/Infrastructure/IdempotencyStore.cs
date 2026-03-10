using System.Collections.Concurrent;

namespace JoiabagurPV.API.Infrastructure;

/// <summary>
/// In-memory idempotency store for preventing duplicate bulk submissions.
/// Keys expire after 24 hours.
/// </summary>
public static class IdempotencyStore
{
    private static readonly ConcurrentDictionary<string, (object? Result, DateTime CreatedAt)> _store = new();
    private static readonly TimeSpan Expiration = TimeSpan.FromHours(24);

    public static (bool Exists, object? Result) TryGet(string key)
    {
        Cleanup();
        if (_store.TryGetValue(key, out var entry))
        {
            if (DateTime.UtcNow - entry.CreatedAt > Expiration)
            {
                _store.TryRemove(key, out _);
                return (false, null);
            }
            return (true, entry.Result);
        }
        return (false, null);
    }

    public static void Reserve(string key)
    {
        _store.TryAdd(key, (null, DateTime.UtcNow));
    }

    public static void Store(string key, object result)
    {
        _store[key] = (result, DateTime.UtcNow);
    }

    public static void Remove(string key)
    {
        _store.TryRemove(key, out _);
    }

    private static void Cleanup()
    {
        var cutoff = DateTime.UtcNow - Expiration;
        foreach (var kvp in _store)
        {
            if (kvp.Value.CreatedAt < cutoff)
            {
                _store.TryRemove(kvp.Key, out _);
            }
        }
    }
}
