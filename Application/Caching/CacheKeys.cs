namespace Application.Caching;

// Shared so Task 1 (writes) and Task 2 (invalidates) can never build the key differently.
public static class CacheKeys
{
    public static string ForIp(string address) => $"ipinfo:{address}";
}
