namespace Infrastructure.Configuration;

// Mirrors the "Ip2C" section in appsettings.json. Whoever builds IIp2CClient should
// inject IOptions<Ip2COptions> instead of reading configuration directly or
// hardcoding the URL - that way the base URL only ever lives in one place.
public class Ip2COptions
{
    public const string SectionName = "Ip2C";

    public string BaseUrl { get; set; } = string.Empty;
}
