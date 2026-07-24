using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Δομημένη, parsed μορφή μιας ακατέργαστης γραμμής απάντησης του IP2C
/// ("status;countryCode2;countryCode3;countryName"), ώστε να μη διαρρέει ποτέ σκέτο
/// string στο υπόλοιπο σύστημα.
/// </summary>
public record Ip2CResult(
    Ip2CStatus Status,
    string? TwoLetterCode,
    string? ThreeLetterCode,
    string? CountryName)
{
    /// <summary>True μόνο όταν η IP επιλύθηκε επιτυχώς.</summary>
    public bool IsSuccess => Status == Ip2CStatus.Success;
}
