namespace Application.BackgroundJobs;

///Ta settings tou task2 dld otan tha thelw mesa ston kwdika na allaksw tin wra i to batch
///den tha piasw kathe file na allaksw tis times alla tha mpw sto settings na ta allaksw gt exw kanei ta panta metavlites

/// sealed gt kanenas den prepei na kanei inherit settings
public sealed class IpRefreshOptions
{
    ///The configuration section these settings are bound from. To file 'Iprefresh pou ftiaxnw argotera'
    public const string SectionName = "IpUpdateJob";

    ///How often the job runs. Defaults to once per hour.
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1);

    ///How many IPs to pull and process per batch. Defaults to 100.
    public int BatchSize { get; set; } = 100;
}