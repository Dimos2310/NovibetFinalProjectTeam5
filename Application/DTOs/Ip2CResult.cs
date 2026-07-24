using Domain.Enums;

namespace Application.DTOs;

public record Ip2CResult(
    Ip2CStatus Status,
    string? TwoLetterCode,
    string? ThreeLetterCode,
    string? CountryName)
{
    public bool IsSuccess => Status == Ip2CStatus.Success;
}
