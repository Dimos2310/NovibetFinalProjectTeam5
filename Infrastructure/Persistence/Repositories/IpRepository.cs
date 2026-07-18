using Application.Abstractions;
using Application.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

// EF Core implementation of IIpRepository - same idea as CountryRepository.
public class IpRepository : IIpRepository
{
    private readonly AppDbContext _context;

    public IpRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ip?> GetByAddressAsync(string address, CancellationToken ct = default)
        => await _context.Ips
            .FirstOrDefaultAsync(i => i.Address == address, ct);

    public async Task<Ip> AddAsync(Ip ip, CancellationToken ct = default)
    {
        // This only stages the new Ip in memory - it does NOT hit the database yet.
        // Task 1's flow adds a Country and an Ip together and wants to save both at
        // once, so saving is a separate step (SaveChangesAsync below) the caller controls.
        await _context.Ips.AddAsync(ip, ct);
        return ip;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<Ip>> GetBatchAsync(int skip, int take, CancellationToken ct = default)
        => await _context.Ips
            .AsNoTracking() // we're only reading these, not going to change and save them, so this skips some EF bookkeeping and is faster
            .OrderBy(i => i.Id) // need a stable order, otherwise skip/take could return overlapping or missing rows between calls
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CountryReportItem>> GetReportAsync(
        string[]? countryCodes,
        CancellationToken ct = default)
    {
        // Joins every Ip to its Country so we can group by country name below.
        // This whole query runs as SQL on the database - we're not loading every
        // row into memory and grouping it ourselves in C#.
        var query =
            from ip in _context.Ips.AsNoTracking()
            join country in _context.Countries.AsNoTracking()
                on ip.CountryTwoLetterCode equals country.TwoLetterCode
            select new { ip.LastUpdated, country.TwoLetterCode, country.CountryName };

        // Only filter if the caller actually passed specific codes - null/empty means "all countries".
        if (countryCodes is { Length: > 0 })
        {
            query = query.Where(x => countryCodes.Contains(x.TwoLetterCode));
        }

        // One group per country: count how many Ip rows landed in it, and find the
        // most recent LastUpdated among them - exactly the shape Task 3 asks for.
        return await query
            .GroupBy(x => x.CountryName)
            .Select(g => new CountryReportItem(
                g.Key,
                g.Count(),
                g.Max(x => x.LastUpdated)))
            .ToListAsync(ct);
    }
}
