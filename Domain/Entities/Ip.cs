namespace Domain.Entities;

/// <summary>
/// Aggregate root that represents a single IP address and the country it resolves to.
/// The link to <see cref="Country"/> is kept by value (two-letter code) so the two
/// aggregates stay independent.
/// </summary>
public class Ip
{
    public int Id { get; set; }

    /// <summary>The IP address (e.g. "212.51.34.10").</summary>
    public required string Address { get; set; }

    /// <summary>The two-letter code of the country this IP resolves to.</summary>
    public required string CountryTwoLetterCode { get; set; }

    /// <summary>
    /// When this IP was last refreshed from IP2C. Feeds both the hourly update job
    /// (Task 2) and the "LastAddressUpdated" column of the report (Task 3).
    /// </summary>
    public DateTime LastUpdated { get; set; }
}
