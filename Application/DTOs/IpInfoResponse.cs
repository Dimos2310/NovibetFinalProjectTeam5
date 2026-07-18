namespace Application.DTOs;

/// <summary>
/// Response contract of the "IP information" endpoint (Task 1).
/// Kept separate from the <c>Country</c> entity so the public API shape and the
/// database schema can evolve independently.
/// </summary>
public record IpInfoResponse(
    string CountryName,
    string TwoLetterCode,
    string ThreeLetterCode);
