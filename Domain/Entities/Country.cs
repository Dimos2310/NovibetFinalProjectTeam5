namespace Domain.Entities;

/// <summary>
/// Aggregate root that represents a country as returned by the IP2C service.
/// The two-letter ISO code is the natural key used to link an <see cref="Ip"/> to its country.
/// </summary>
public class Country
{
    public int Id { get; set; }

    /// <summary>ISO 3166-1 alpha-2 code (e.g. "GR"). Natural key.</summary>
    public required string TwoLetterCode { get; set; }

    /// <summary>ISO 3166-1 alpha-3 code (e.g. "GRC").</summary>
    public required string ThreeLetterCode { get; set; }

    /// <summary>Human-readable country name (e.g. "Greece").</summary>
    public required string CountryName { get; set; }
}
