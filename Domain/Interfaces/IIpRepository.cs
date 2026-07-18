using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
/// Persistence contract for <see cref="Ip"/> aggregates. Implemented in Infrastructure (EF Core).
/// The domain declares WHAT it needs; Infrastructure decides HOW.
/// </summary>
public interface IIpRepository
{
    /// <summary>Returns the IP with the given address, or null if it is not persisted yet.</summary>
    Task<Ip?> GetByAddressAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>Persists a new IP.</summary>
    Task AddAsync(Ip ip, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing IP (e.g. after the hourly refresh).</summary>
    Task UpdateAsync(Ip ip, CancellationToken cancellationToken = default);

    /// <summary>Total number of persisted IPs (used by the update job to page through them).</summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns a single batch of IPs for the periodic update job (Task 2).</summary>
    Task<IReadOnlyList<Ip>> GetBatchAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>Returns all IPs, optionally filtered by country codes (used by the report, Task 3).</summary>
    Task<IReadOnlyList<Ip>> GetAllAsync(
        IReadOnlyCollection<string>? twoLetterCodes = null,
        CancellationToken cancellationToken = default);
}
