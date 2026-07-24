namespace Domain.Entities;

// One row in the "Countries" table.
public class Country
{
<<<<<<< HEAD
    // The real primary key (see AppDbContext.OnModelCreating) - Country doesn't have a
    // separate int Id at all, because a 2-letter ISO code is already unique on its own.
=======

    // The 2-letter code (e.g. "GR") is what we actually use to identify a country
    // everywhere else in the app - it's set as the real primary key in the DB config,
    // Id above just exists because EF likes having one, but we don't use it for anything.
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
    public required string TwoLetterCode { get; set; }

    // The 3-letter version of the same code, e.g. "GRC" for Greece. Just a display/lookup
    // field - TwoLetterCode is what everything else in the app actually keys off of.
    public required string ThreeLetterCode { get; set; }

    public required string CountryName { get; set; }
}
