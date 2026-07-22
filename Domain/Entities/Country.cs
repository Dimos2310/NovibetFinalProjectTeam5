namespace Domain.Entities;

// This class represents one row in the "Countries" table.
public class Country
{

    // The 2-letter code (e.g. "GR") is what we actually use to identify a country
    // everywhere else in the app - it's set as the real primary key in the DB config,
    // Id above just exists because EF likes having one, but we don't use it for anything.
    public required string TwoLetterCode { get; set; }

    public required string ThreeLetterCode { get; set; }

    public required string CountryName { get; set; }
}
