using Application.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly AppDbContext _context;

    public CountryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Country?> GetByCodeAsync(string twoLetterCode, CancellationToken ct = default)
        => await _context.Countries
            .FirstOrDefaultAsync(c => c.TwoLetterCode == twoLetterCode, ct);

    public async Task AddIfNotExistsAsync(Country country, CancellationToken ct = default)
    {
        var exists = await _context.Countries
            .AnyAsync(c => c.TwoLetterCode == country.TwoLetterCode, ct);

        if (exists)
            return;

        _context.Countries.Add(country);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Two concurrent requests both saw "doesn't exist" and both inserted; the
            // unique key let one win. The country exists either way, so just detach ours.
            _context.Entry(country).State = EntityState.Detached;
        }
    }
}
