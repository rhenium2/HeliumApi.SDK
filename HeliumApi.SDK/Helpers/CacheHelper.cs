using System.Linq.Expressions;
using LocalObjectCache;

namespace HeliumApi.SDK.Helpers;

public static class CacheHelper
{
    private static readonly Cache _cache;

    static CacheHelper()
    {
        _cache = new Cache("cache.db", CacheOptions.CacheValidity);
    }

    public static T? GetOne<T>(Expression<Func<T, bool>> predicate)
    {
        if (!CacheOptions.DisabledTypes.Contains(typeof(T)))
        {
            return _cache.GetOne<T>(predicate);
        }

        return default;
    }

    public static List<T> GetMany<T>(Expression<Func<T, bool>> predicate)
    {
        if (!CacheOptions.DisabledTypes.Contains(typeof(T)))
        {
            return _cache.GetMany<T>(predicate);
        }

        return new List<T>();
    }

    public static void InsertOne<T>(T item)
    {
        if (!CacheOptions.DisabledTypes.Contains(typeof(T)))
        {
            _cache.InsertOne(item);
        }
    }

    public static void InsertMany<T>(IEnumerable<T> items)
    {
        if (!CacheOptions.DisabledTypes.Contains(typeof(T)))
        {
            _cache.InsertMany(items);
        }
    }
}