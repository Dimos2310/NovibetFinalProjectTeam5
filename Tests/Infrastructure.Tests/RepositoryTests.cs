using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests;

public class CountryRepositoryTests : IClassFixture<SqliteTestDatabase>
{
    private readonly SqliteTestDatabase _db;

    public CountryRepositoryTests(SqliteTestDatabase db) => _db = db;

    [Fact]
    public async Task AddIfNotExists_then_GetByCode_round_trips()
    {
        await using var context = _db.CreateContext();
        ICountryRepository repo = new CountryRepository(context);

        var greece = new Country { TwoLetterCode = "GR", ThreeLetterCode = "GRC", CountryName = "Greece" };
        await repo.AddIfNotExistsAsync(greece);

        var found = await repo.GetByCodeAsync("GR");

        Assert.NotNull(found);
        Assert.Equal("Greece", found!.CountryName);
        Assert.Equal("GRC", found.ThreeLetterCode);
    }

    [Fact]
    public async Task AddIfNotExists_is_idempotent_for_the_same_code()
    {
        await using var context = _db.CreateContext();
        ICountryRepository repo = new CountryRepository(context);

        await repo.AddIfNotExistsAsync(new Country { TwoLetterCode = "IT", ThreeLetterCode = "ITA", CountryName = "Italy" });
        await repo.AddIfNotExistsAsync(new Country { TwoLetterCode = "IT", ThreeLetterCode = "ITA", CountryName = "Italy" });

        await using var verifyContext = _db.CreateContext();
        var count = await verifyContext.Countries.CountAsync(c => c.TwoLetterCode == "IT");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task AddIfNotExists_survives_a_genuine_race_on_the_same_code()
    {
        // Two separate DbContext instances, as two concurrent requests would have,
        // both inserting the same brand-new code without seeing each other first.
        await using var contextA = _db.CreateContext();
        await using var contextB = _db.CreateContext();
        ICountryRepository repoA = new CountryRepository(contextA);
        ICountryRepository repoB = new CountryRepository(contextB);

        var country = new Country { TwoLetterCode = "FR", ThreeLetterCode = "FRA", CountryName = "France" };

        await repoA.AddIfNotExistsAsync(country);
        await repoB.AddIfNotExistsAsync(
            new Country { TwoLetterCode = "FR", ThreeLetterCode = "FRA", CountryName = "France" });

        await using var verifyContext = _db.CreateContext();
        var count = await verifyContext.Countries.CountAsync(c => c.TwoLetterCode == "FR");
        Assert.Equal(1, count);
    }
}

public class IpRepositoryTests : IClassFixture<SqliteTestDatabase>
{
    private readonly SqliteTestDatabase _db;

    public IpRepositoryTests(SqliteTestDatabase db) => _db = db;

    [Fact]
    public async Task Add_then_SaveChanges_then_GetByAddress_round_trips()
    {
        await using var context = _db.CreateContext();
        ICountryRepository countryRepo = new CountryRepository(context);
        IIpRepository ipRepo = new IpRepository(context);

        await countryRepo.AddIfNotExistsAsync(
            new Country { TwoLetterCode = "GR", ThreeLetterCode = "GRC", CountryName = "Greece" });

        var ip = new Ip { Address = "1.2.3.4", CountryTwoLetterCode = "GR", LastUpdated = DateTime.UtcNow };
        await ipRepo.AddAsync(ip);
        await ipRepo.SaveChangesAsync();

        var found = await ipRepo.GetByAddressAsync("1.2.3.4");
        Assert.NotNull(found);
        Assert.Equal("GR", found!.CountryTwoLetterCode);
    }

    [Fact]
    public async Task GetBatch_pages_through_results_in_order()
    {
        await using var context = _db.CreateContext();
        ICountryRepository countryRepo = new CountryRepository(context);
        IIpRepository ipRepo = new IpRepository(context);

        await countryRepo.AddIfNotExistsAsync(
            new Country { TwoLetterCode = "ES", ThreeLetterCode = "ESP", CountryName = "Spain" });

        for (var i = 0; i < 5; i++)
        {
            await ipRepo.AddAsync(new Ip
            {
                Address = $"10.0.0.{i}",
                CountryTwoLetterCode = "ES",
                LastUpdated = DateTime.UtcNow
            });
        }
        await ipRepo.SaveChangesAsync();

        var firstPage = await ipRepo.GetBatchAsync(skip: 0, take: 2);
        var secondPage = await ipRepo.GetBatchAsync(skip: 2, take: 2);

        Assert.Equal(2, firstPage.Count);
        Assert.Equal(2, secondPage.Count);
        Assert.NotEqual(firstPage[0].Address, secondPage[0].Address);
    }

    [Fact]
    public async Task GetReport_groups_by_country_counts_and_returns_latest_update()
    {
        await using var context = _db.CreateContext();
        ICountryRepository countryRepo = new CountryRepository(context);
        IIpRepository ipRepo = new IpRepository(context);

        await countryRepo.AddIfNotExistsAsync(
            new Country { TwoLetterCode = "CY", ThreeLetterCode = "CYP", CountryName = "Cyprus" });

        var older = DateTime.UtcNow.AddHours(-2);
        var newer = DateTime.UtcNow;

        await ipRepo.AddAsync(new Ip { Address = "9.9.9.1", CountryTwoLetterCode = "CY", LastUpdated = older });
        await ipRepo.AddAsync(new Ip { Address = "9.9.9.2", CountryTwoLetterCode = "CY", LastUpdated = newer });
        await ipRepo.SaveChangesAsync();

        var report = await ipRepo.GetReportAsync(countryCodes: null);
        var cyprusRow = Assert.Single(report, r => r.CountryName == "Cyprus");

        Assert.Equal(2, cyprusRow.AddressesCount);
        Assert.Equal(newer, cyprusRow.LastAddressUpdated, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetReport_filters_by_the_requested_country_codes()
    {
        await using var context = _db.CreateContext();
        ICountryRepository countryRepo = new CountryRepository(context);
        IIpRepository ipRepo = new IpRepository(context);

        await countryRepo.AddIfNotExistsAsync(
            new Country { TwoLetterCode = "DE", ThreeLetterCode = "DEU", CountryName = "Germany" });
        await countryRepo.AddIfNotExistsAsync(
            new Country { TwoLetterCode = "JP", ThreeLetterCode = "JPN", CountryName = "Japan" });

        await ipRepo.AddAsync(new Ip { Address = "8.8.8.1", CountryTwoLetterCode = "DE", LastUpdated = DateTime.UtcNow });
        await ipRepo.AddAsync(new Ip { Address = "8.8.8.2", CountryTwoLetterCode = "JP", LastUpdated = DateTime.UtcNow });
        await ipRepo.SaveChangesAsync();

        var report = await ipRepo.GetReportAsync(countryCodes: new[] { "DE" });

        Assert.Single(report);
        Assert.Equal("Germany", report[0].CountryName);
    }
}
