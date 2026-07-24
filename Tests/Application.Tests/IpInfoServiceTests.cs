using Application.DTOs;
using Application.Exceptions;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests;

public class IpInfoServiceTests
{
    private static readonly Country Greece = new()
    {
        TwoLetterCode = "GR",
        ThreeLetterCode = "GRC",
        CountryName = "Greece"
    };

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
        Assert.Equal(0, ip2c.CallCount);
    }

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
        Assert.Equal(0, ip2c.CallCount);
        Assert.Equal(1, cache.SetCallCount);
    }

    [Fact]
    public async Task Full_miss_with_ip2c_success_persists_country_and_ip_then_caches_the_response()
    {
        var cache = new FakeCacheService();
        var ipRepo = new InMemoryIpRepository();
        var countryRepo = new FakeCountryRepository();
        var ip2c = new FakeIp2CClient(new Ip2CResult(Ip2CStatus.Success, "GR", "GRC", "Greece"));
        var sut = new IpInfoService(cache, countryRepo, ipRepo, ip2c, NullLogger<IpInfoService>.Instance);

        var result = await sut.GetIpInfoAsync("9.9.9.9");

        Assert.NotNull(result);
        Assert.Equal(new IpInfoResponse("Greece", "GR", "GRC"), result);
        Assert.Equal(1, ip2c.CallCount);
        Assert.Equal(1, countryRepo.AddIfNotExistsCallCount);
        Assert.Equal(1, ipRepo.SaveChangesCallCount);
        Assert.Equal(1, cache.SetCallCount);

        // Second call should now be served from cache, without calling IP2C again.
        var second = await sut.GetIpInfoAsync("9.9.9.9");
        Assert.Equal(result, second);
        Assert.Equal(1, ip2c.CallCount);
    }

    [Fact]
    public async Task Full_miss_with_ip2c_unknown_returns_null_and_persists_nothing()
    {
        var cache = new FakeCacheService();
        var ipRepo = new InMemoryIpRepository();
        var countryRepo = new FakeCountryRepository();
        var ip2c = new FakeIp2CClient(new Ip2CResult(Ip2CStatus.Unknown, null, null, null));
        var sut = new IpInfoService(cache, countryRepo, ipRepo, ip2c, NullLogger<IpInfoService>.Instance);

        var result = await sut.GetIpInfoAsync("203.0.113.1");

        Assert.Null(result);
        Assert.Equal(0, countryRepo.AddIfNotExistsCallCount);
        Assert.Equal(0, ipRepo.SaveChangesCallCount);
        Assert.Equal(0, cache.SetCallCount);
    }

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

        Assert.Equal(0, ipRepo.SaveChangesCallCount);
    }
}
