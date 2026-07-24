using Application.Abstractions;
using Application.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

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
        await _context.Ips.AddAsync(ip, ct);
        return ip;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<Ip>> GetBatchAsync(int skip, int take, CancellationToken ct = default)
        => await _context.Ips
            .AsNoTracking()
            .OrderBy(i => i.Id) // stable order so skip/take can't overlap or skip rows
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CountryReportItem>> GetReportAsync(
        string[]? countryCodes,
        CancellationToken ct = default)
    {
        var query =
            from ip in _context.Ips.AsNoTracking()
            join country in _context.Countries.AsNoTracking()
                on ip.CountryTwoLetterCode equals country.TwoLetterCode
            select new { ip.LastUpdated, country.TwoLetterCode, country.CountryName };

        if (countryCodes is { Length: > 0 })
        {
            query = query.Where(x => countryCodes.Contains(x.TwoLetterCode));
        }

        return await query
            .GroupBy(x => x.CountryName)
            .Select(g => new CountryReportItem(
                g.Key,
                g.Count(),
                g.Max(x => x.LastUpdated)))
            .ToListAsync(ct);
    }
}
