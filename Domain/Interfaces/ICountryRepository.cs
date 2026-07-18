using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Persistence contract for <see cref="Country"/> aggregates. Implemented in Infrastructure (EF Core).
/// </summary>
public interface ICountryRepository
{
    /// <summary>Returns the country with the given two-letter code, or null if unknown.</summary>
    Task<Country?> GetByTwoLetterCodeAsync(string twoLetterCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the existing country for the given two-letter code, or persists and returns
    /// the supplied one if it is not stored yet. Keeps countries de-duplicated.
    /// </summary>
    Task<Country> GetOrAddAsync(Country country, CancellationToken cancellationToken = default);

    /// <summary>Returns all known countries (used by the report to resolve names).</summary>
    Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken = default);
}
