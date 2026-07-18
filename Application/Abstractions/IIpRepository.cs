using Application.DTOs;
using Domain.Entities;

namespace Application.Abstractions;

// Contract for talking to the Ips table. Same idea as ICountryRepository -
// whoever builds the endpoints/job codes against this interface, not against EF directly.
public interface IIpRepository
{
    // Looks up an IP we've already seen before. Null means "never stored this one" -
    // that's Task 1's cue to fall through to calling IP2C.
    Task<Ip?> GetByAddressAsync(string address, CancellationToken ct = default);

    // Adds a new Ip, but does NOT save it to the database yet - see SaveChangesAsync below.
    Task<Ip> AddAsync(Ip ip, CancellationToken ct = default);

    // Actually writes whatever's pending (from AddAsync, or from changing an Ip you
    // already fetched) to the database. Kept separate from AddAsync so a caller can
    // add/change several things and save them all in one go instead of one DB trip each.
    Task SaveChangesAsync(CancellationToken ct = default);

    // Returns one "page" of IPs (skip/take), used by the hourly job so it doesn't have
    // to pull every single IP into memory at once - it processes them 100 at a time.
    Task<IReadOnlyList<Ip>> GetBatchAsync(int skip, int take, CancellationToken ct = default);

    // Powers the report endpoint (Task 3): how many IPs per country, and when the
    // newest one was updated. Pass null to get every country, or specific codes to filter.
    Task<IReadOnlyList<CountryReportItem>> GetReportAsync(string[]? countryCodes, CancellationToken ct = default);
}
