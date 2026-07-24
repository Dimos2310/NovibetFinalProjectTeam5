namespace Application.BackgroundJobs;

public sealed class IpRefreshOptions
{
    public const string SectionName = "IpUpdateJob";

    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    public int BatchSize { get; set; } = 100;
}
