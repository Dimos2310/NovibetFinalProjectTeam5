using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests;

// Το IpInfoService έχει τρία μονοπάτια (cache hit / DB hit / IP2C) και τρία αποτελέσματα
// από το IP2C (success / unknown / invalid) - αυτά τα tests ελέγχουν ότι το κάθε ένα
// σταματάει στο σωστό σημείο και δεν "πέφτει" περιττά στα ακριβότερα βήματα.
public class IpInfoServiceTests
{
    private static readonly Country Greece = new()
    {
        TwoLetterCode = "GR",
        ThreeLetterCode = "GRC",
        CountryName = "Greece"
    };

    // ---------- (1) cache hit ----------

    [Fact]
    public async Task Cache_hit_returns_immediately_without_touching_db_or_ip2c()
    {
        var cache = new FakeCacheService();
        await cache.SetAsync("ipinfo:1.2.3.4", new IpInfoResponse("Greece", "GR", "GRC"));

        var ipRepo = new InMemoryIpRepository();
        var ip2c = new FakeIp2CClient(new Ip2CResult(Ip2CStatus.Success, "GR", "GRC", "Greece"));
        var sut = new IpInfoService(cache, new FakeCountryRepository(), ipRepo, ip2c, NullLogger<IpInfoService>.Instance);

        var result = await sut.GetIpInfoAsync("1.2.3.4");

        Assert.NotNull(result);
        Assert.Equal("Greece", result!.CountryName);
        Assert.Equal(0, ip2c.CallCount); // δεν έπρεπε καν να χρειαστεί να ρωτήσει το IP2C
    }

    // ---------- (2) DB hit ----------

    [Fact]
    public async Task Db_hit_returns_from_the_database_populates_cache_and_skips_ip2c()
    {
        var cache = new FakeCacheService();
        var ipRepo = new InMemoryIpRepository(new Ip
        {
            Address = "5.6.7.8",
            CountryTwoLetterCode = "GR",
            LastUpdated = DateTime.UtcNow
        });
        var countryRepo = new FakeCountryRepository(Greece);
        var ip2c = new FakeIp2CClient(new Ip2CResult(Ip2CStatus.Success, "GR", "GRC", "Greece"));
        var sut = new IpInfoService(cache, countryRepo, ipRepo, ip2c, NullLogger<IpInfoService>.Instance);

        var result = await sut.GetIpInfoAsync("5.6.7.8");

        Assert.NotNull(result);
        Assert.Equal("GR", result!.TwoLetterCode);
        Assert.Equal(0, ip2c.CallCount);   // η βάση είχε ήδη την απάντηση
        Assert.Equal(1, cache.SetCallCount); // write-through: η επόμενη φορά πιάνει cache
    }

    // ---------- (3) cache+DB miss, IP2C success ----------

    [Fact]
    public async Task Full_miss_with_ip2c_success_persists_country_and_ip_then_caches_the_response()
    {
        var cache = new FakeCacheService();
        var ipRepo = new InMemoryIpRepository();
        var countryRepo = new FakeCountryRepository(); // άδειο - δεν την ξέρουμε ακόμα
        var ip2c = new FakeIp2CClient(new Ip2CResult(Ip2CStatus.Success, "GR", "GRC", "Greece"));
        var sut = new IpInfoService(cache, countryRepo, ipRepo, ip2c, NullLogger<IpInfoService>.Instance);

        var result = await sut.GetIpInfoAsync("9.9.9.9");

        Assert.NotNull(result);
        Assert.Equal(new IpInfoResponse("Greece", "GR", "GRC"), result);
        Assert.Equal(1, ip2c.CallCount);
        Assert.Equal(1, countryRepo.AddIfNotExistsCallCount); // η χώρα αποθηκεύτηκε
        Assert.Equal(1, ipRepo.SaveChangesCallCount);         // και το ίδιο το Ip
        Assert.Equal(1, cache.SetCallCount);                  // και μπήκε στην cache

        // Δεύτερη κλήση: τώρα πρέπει να πιάνει cache και να μην ξαναρωτάει το IP2C.
        var second = await sut.GetIpInfoAsync("9.9.9.9");
        Assert.Equal(result, second);
        Assert.Equal(1, ip2c.CallCount); // ίδιο με πριν - δεν αυξήθηκε
    }

    // ---------- (4) cache+DB miss, IP2C unknown ----------

    [Fact]
    public async Task Full_miss_with_ip2c_unknown_returns_null_and_persists_nothing()
    {
        var cache = new FakeCacheService();
        var ipRepo = new InMemoryIpRepository();
        var countryRepo = new FakeCountryRepository();
        var ip2c = new FakeIp2CClient(new Ip2CResult(Ip2CStatus.Unknown, null, null, null));
        var sut = new IpInfoService(cache, countryRepo, ipRepo, ip2c, NullLogger<IpInfoService>.Instance);

        var result = await sut.GetIpInfoAsync("203.0.113.1");

        Assert.Null(result); // ο caller (IpController) το μεταφράζει σε 404
        Assert.Equal(0, countryRepo.AddIfNotExistsCallCount);
        Assert.Equal(0, ipRepo.SaveChangesCallCount);
        Assert.Equal(0, cache.SetCallCount); // δεν υπάρχει τι να μπει στην cache
    }

    // ---------- (5) cache+DB miss, IP2C invalid ----------

    [Fact]
    public async Task Full_miss_with_ip2c_invalid_throws_so_the_middleware_can_return_400()
    {
        var cache = new FakeCacheService();
        var ipRepo = new InMemoryIpRepository();
        var countryRepo = new FakeCountryRepository();
        var ip2c = new FakeIp2CClient(new Ip2CResult(Ip2CStatus.Invalid, null, null, null));
        var sut = new IpInfoService(cache, countryRepo, ipRepo, ip2c, NullLogger<IpInfoService>.Instance);

        await Assert.ThrowsAsync<InvalidIpAddressException>(
            () => sut.GetIpInfoAsync("not-an-ip"));

        Assert.Equal(0, ipRepo.SaveChangesCallCount); // τίποτα δεν αποθηκεύτηκε
    }
}
