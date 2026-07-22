namespace Infrastructure.Configuration;

// Mirrors the "IpUpdateJob" section in appsettings.json. The hourly background job
// (Task 2) should inject IOptions<IpUpdateJobOptions> to know how often to run and
// how many IPs to process per batch, instead of hardcoding either value.
public class IpUpdateJobOptions
{
    public const string SectionName = "IpUpdateJob";

    public int IntervalHours { get; set; } = 1;
    public int BatchSize { get; set; } = 100;
}
