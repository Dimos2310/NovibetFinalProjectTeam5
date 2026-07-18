namespace Domain.Entities;

// This class represents one row in the "Ips" table.
public class Ip
{
    public int Id { get; set; }

    // The actual IP address, e.g. "212.51.34.10". This has a unique index in the DB
    // config, so the same address can never be stored twice.
    public required string Address { get; set; }

    // Points at the Country this IP belongs to, by its 2-letter code (not by Id).
    // This is what the foreign key in the DB config is built on.
    public required string CountryTwoLetterCode { get; set; }

    // The last time we checked this IP against IP2C. Used by the hourly job to know
    // what's stale, and by the report to show "LastAddressUpdated".
    public DateTime LastUpdated { get; set; }
}
