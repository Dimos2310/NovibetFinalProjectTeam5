using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Ip> Ips => Set<Ip>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries");

            // Natural key - a 2-letter ISO code is already unique on its own.
            entity.HasKey(c => c.TwoLetterCode);
            entity.Property(c => c.TwoLetterCode).HasMaxLength(2).IsFixedLength();

            entity.Property(c => c.ThreeLetterCode).IsRequired().HasMaxLength(3).IsFixedLength();
            entity.Property(c => c.CountryName).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Ip>(entity =>
        {
            entity.ToTable("Ips");

            entity.HasKey(i => i.Id);
            entity.Property(i => i.Address).IsRequired().HasMaxLength(45); // fits IPv6

            entity.HasIndex(i => i.Address).IsUnique();

            entity.Property(i => i.CountryTwoLetterCode).IsRequired().HasMaxLength(2).IsFixedLength();

            entity.HasOne<Country>()
                  .WithMany()
                  .HasForeignKey(i => i.CountryTwoLetterCode);

            entity.Property(i => i.LastUpdated).IsRequired();
        });
    }
}
