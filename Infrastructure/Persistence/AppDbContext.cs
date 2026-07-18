using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

// This is EF Core's "session" with the database - it knows about our two tables
// and how they're shaped. Everything below only runs once, when EF builds the model
// (not on every query), so it's fine that it looks like a lot of setup code.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // These two properties are how the rest of the app actually queries the tables,
    // e.g. _context.Countries.Where(...). Set<T>() just means "give me the table for T".
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Ip> Ips => Set<Ip>();

    // EF calls this automatically when it builds the model - this is where we describe
    // keys, required fields, max lengths, and relationships (things that don't fit as
    // plain C# properties on the entity classes themselves).
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries");

            // TwoLetterCode is the real primary key here, not the Id property - a country
            // code is already unique on its own, so we don't need a separate int id for it.
            entity.HasKey(c => c.TwoLetterCode);
            entity.Property(c => c.TwoLetterCode).HasMaxLength(2).IsFixedLength();

            entity.Property(c => c.ThreeLetterCode).IsRequired().HasMaxLength(3).IsFixedLength();
            entity.Property(c => c.CountryName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Ip>(entity =>
        {
            entity.ToTable("Ips");

            // Here we DO use the int Id as the primary key (unlike Country above) - Address
            // could have worked too, but a plain auto-incrementing int is simpler to index.
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Address).IsRequired().HasMaxLength(45); // 45 chars comfortably fits IPv6

            // This is what stops the same IP address from ever being stored twice, even if
            // two requests try to insert it at the same moment (see CountryRepository /
            // IpRepository for how we handle that race in code).
            entity.HasIndex(i => i.Address).IsUnique();

            entity.Property(i => i.CountryTwoLetterCode).IsRequired().HasMaxLength(2).IsFixedLength();

            // Links each Ip row to a Country row via CountryTwoLetterCode - this is what
            // makes it an actual foreign key in the database, not just a matching string.
            entity.HasOne<Country>()
                  .WithMany()
                  .HasForeignKey(i => i.CountryTwoLetterCode);

            entity.Property(i => i.LastUpdated).IsRequired();
        });
    }
}
