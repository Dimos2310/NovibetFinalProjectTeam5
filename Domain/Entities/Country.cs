namespace Domain.Entities;

// One row in the "Countries" table.
public class Country
{
    // The real primary key (see AppDbContext.OnModelCreating) - Country doesn't have a
    // separate int Id at all, because a 2-letter ISO code is already unique on its own.
    public required string TwoLetterCode { get; set; }

    // The 3-letter version of the same code, e.g. "GRC" for Greece. Just a display/lookup
    // field - TwoLetterCode is what everything else in the app actually keys off of.
    public required string ThreeLetterCode { get; set; }

    public required string CountryName { get; set; }
}
