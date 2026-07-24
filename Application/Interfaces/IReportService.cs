using Application.DTOs;

namespace Application.Interfaces;

public interface IReportService
{
    // Null or empty twoLetterCodes returns every country.
    Task<IReadOnlyList<CountryReportItem>> GetReportAsync(
        string[]? twoLetterCodes,
        CancellationToken cancellationToken = default);
}
