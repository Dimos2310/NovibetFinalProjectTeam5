using Domain.Entities;

namespace Application.Abstractions;

public interface ICountryRepository
{
    Task<Country?> GetByCodeAsync(string twoLetterCode, CancellationToken ct = default);

    Task AddIfNotExistsAsync(Country country, CancellationToken ct = default);
}
