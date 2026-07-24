namespace Infrastructure.Configuration;

public class CacheOptions
{
    public const string SectionName = "Cache";

    public int TtlMinutes { get; set; } = 60;
}
