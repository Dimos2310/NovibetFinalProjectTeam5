namespace Application.DTOs;

public record IpInfoResponse(
    string CountryName,
    string TwoLetterCode,
    string ThreeLetterCode);
