using Application.DTOs;
using Domain.Entities;

namespace Application.Abstractions;

public interface IIpRepository
{
    Task<Ip?> GetByAddressAsync(string address, CancellationToken ct = default);

    // Does not save - see SaveChangesAsync.
    Task<Ip> AddAsync(Ip ip, CancellationToken ct = default);

    // Kept separate from AddAsync so a caller can batch several changes into one save.
    Task SaveChangesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Ip>> GetBatchAsync(int skip, int take, CancellationToken ct = default);

    Task<IReadOnlyList<CountryReportItem>> GetReportAsync(string[]? countryCodes, CancellationToken ct = default);
}
