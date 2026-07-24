using Application.Abstractions;
using Application.DTOs;
using Application.Interfaces;

namespace Application.Services;

public class ReportService : IReportService
{
    private readonly IIpRepository _ipRepository;

    public ReportService(IIpRepository ipRepository)
    {
        _ipRepository = ipRepository;
    }

    public async Task<IReadOnlyList<CountryReportItem>> GetReportAsync(
        string[]? twoLetterCodes,
        CancellationToken cancellationToken = default)
    {
        var filter = NormalizeCodes(twoLetterCodes);
        var rows = await _ipRepository.GetReportAsync(filter, cancellationToken);

        // Highest count first, alphabetical on ties - otherwise tie order is whatever
        // the database happens to return, which isn't stable across providers or runs.
        return rows
            .OrderByDescending(r => r.AddressesCount)
            .ThenBy(r => r.CountryName, StringComparer.Ordinal)
            .ToList();
    }

    // IP2C returns codes uppercase, so that's how they're stored - normalize the
    // caller's input to match, and to behave the same on SQL Server (case-insensitive
    // collation) as on SQLite in tests (case-sensitive).
    private static string[]? NormalizeCodes(string[]? codes)
    {
        if (codes is null || codes.Length == 0)
        {
            return null; // null/empty means "all countries"
        }

        var cleaned = codes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();

        return cleaned.Length == 0 ? null : cleaned;
    }
}
