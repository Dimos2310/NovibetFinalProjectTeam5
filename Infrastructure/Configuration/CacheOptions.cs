namespace Infrastructure.Configuration;

// Mirrors the "Cache" section in appsettings.json. The ICacheService implementation
// should inject IOptions<CacheOptions> and use TtlMinutes as the default time-to-live
// instead of hardcoding a number.
public class CacheOptions
{
    public const string SectionName = "Cache";

    public int TtlMinutes { get; set; } = 60;
}
