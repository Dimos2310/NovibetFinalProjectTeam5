using Domain.Entities;

namespace Application.Abstractions;

// This is the "contract" for talking to the Countries table. Nothing outside
// Infrastructure needs to know it's actually EF Core / SQL Server behind this -
// the endpoint code just asks for an ICountryRepository and uses these two methods.
public interface ICountryRepository
{
    // Looks a country up by its 2-letter code. Returns null if we don't have it yet
    // (that's the signal to go call IP2C and then add it).
    Task<Country?> GetByCodeAsync(string twoLetterCode, CancellationToken ct = default);

    // Saves a new country, but only if it isn't already there. Safe to call even if
    // two requests try to add the same country at the same time (see CountryRepository).
    Task AddIfNotExistsAsync(Country country, CancellationToken ct = default);
}
