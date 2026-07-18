using Application.Abstractions;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

// This is the actual EF Core implementation of ICountryRepository - the "how", where
// the interface was just the "what". This is the only class that knows we're using EF.
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
        // First check if we already have this country, so we don't insert a duplicate.
        var exists = await _context.Countries
            .AnyAsync(c => c.TwoLetterCode == country.TwoLetterCode, ct);

        if (exists)
            return; // nothing to do - it's already there

        _context.Countries.Add(country);

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // This catch handles a rare timing problem: two requests both checked
            // "exists?" at almost the same moment, both got "no", and both tried to save.
            // The database's unique key (see AppDbContext) only lets one of them succeed -
            // the other one lands here. That's fine: the country exists now either way,
            // which is all this method promises. We just detach our failed copy so EF
            // doesn't keep trying to save something that will never work.
            _context.Entry(country).State = EntityState.Detached;
        }
    }
}
