using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Use-case contract for the report endpoint (Task 3).
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Returns per-country address counts and last-updated timestamps.
    /// Pass a set of two-letter codes to filter, or null to report on all countries.
    /// </summary>
    Task<IReadOnlyList<CountryReportItem>> GetReportAsync(
        string[]? twoLetterCodes,
        CancellationToken cancellationToken = default);
}
