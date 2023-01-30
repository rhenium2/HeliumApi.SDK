// ReSharper disable UnusedMember.Global

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace HeliumApi.SDK;

public static class CacheOptions
{
    internal static List<Type> DisabledTypes = new();
    public static TimeSpan CacheValidity { get; set; } = TimeSpan.FromDays(1);

    public static void DisableCacheFor<T>()
    {
        var type = typeof(T);
        if (!DisabledTypes.Contains(type))
        {
            DisabledTypes.Add(type);
        }
    }
}