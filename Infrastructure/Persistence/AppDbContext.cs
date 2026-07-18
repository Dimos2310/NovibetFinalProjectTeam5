using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Ip> Ips => Set<Ip>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) //protected because no need for external calls
    { //ta parakato einai endeiktika giati perimenoume apo ta paidia,sigoura tha xreiastoune allages flavio check
        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries");
            entity.HasKey(c => c.TwoLetterCode); //theoro oti mporei na einai primary key to TwoLetterCode kathos den xreiazetai epipleon int pk , ta 2 grammata tis xwras einai enough gia anagnorisi kai diakrisi
            entity.Property(c => c.TwoLetterCode).HasMaxLength(2).IsFixedLength();//always to be 2 chars
            entity.Property(c => c.ThreeLetterCode).IsRequired().HasMaxLength(3).IsFixedLength(); //not null same logic as before
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);//not null, max length 100 chars
        });

        modelBuilder.Entity<Ip>(entity =>
        {
            entity.ToTable("Ips");
            entity.HasKey(i => i.Id);//we could use the address as primary key but we want to have an int id for easier handling and indexing flavio check
            entity.Property(i => i.Address).IsRequired().HasMaxLength(45);
            entity.HasIndex(i => i.Address).IsUnique(); //index searchin faster and less computational cost maybe
            entity.Property(i => i.CountryTwoLetterCode).IsRequired().HasMaxLength(2).IsFixedLength();
            entity.HasOne<Country>() //one ip has one country, one country can have many ips
                  .WithMany()
                  .HasForeignKey(i => i.CountryTwoLetterCode);
            entity.Property(i => i.UpdatedAt);
        });
    }
}
