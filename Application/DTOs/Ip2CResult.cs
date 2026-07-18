using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Structured, parsed form of a raw IP2C response line
/// ("status;countryCode2;countryCode3;countryName"), so a raw string never leaks
/// into the rest of the system.
/// </summary>
public record Ip2CResult(
    Ip2CStatus Status,
    string? TwoLetterCode,
    string? ThreeLetterCode,
    string? CountryName)
{
    /// <summary>True only when the IP was resolved successfully.</summary>
    public bool IsSuccess => Status == Ip2CStatus.Success;
}
