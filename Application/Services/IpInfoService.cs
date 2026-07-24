using Application.Abstractions;
using Application.Caching;
using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class IpInfoService : IIpInfoService
{
    private readonly ICacheService _cache;
    private readonly ICountryRepository _countryRepository;
    private readonly IIpRepository _ipRepository;
    private readonly IIp2CClient _ip2cClient;
    private readonly ILogger<IpInfoService> _logger;

    public IpInfoService(
        ICacheService cache,
        ICountryRepository countryRepository,
        IIpRepository ipRepository,
        IIp2CClient ip2cClient,
        ILogger<IpInfoService> logger)
    {
        _cache = cache;
        _countryRepository = countryRepository;
        _ipRepository = ipRepository;
        _ip2cClient = ip2cClient;
        _logger = logger;
    }

    public async Task<IpInfoResponse?> GetIpInfoAsync(string ip, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.ForIp(ip);

        var cached = await _cache.GetAsync<IpInfoResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var existingIp = await _ipRepository.GetByAddressAsync(ip, cancellationToken);
        if (existingIp is not null)
        {
            var storedCountry = await _countryRepository.GetByCodeAsync(
                existingIp.CountryTwoLetterCode, cancellationToken);

            if (storedCountry is not null)
            {
                var response = ToResponse(storedCountry);
                await _cache.SetAsync(cacheKey, response, cancellationToken: cancellationToken);
                return response;
            }

            // The FK guarantees this shouldn't happen; if the data was edited manually,
            // fall through to IP2C and self-heal instead of failing the request.
            _logger.LogWarning(
                "Ip {Address} referenced country {Code} which no longer exists; re-resolving via IP2C.",
                ip, existingIp.CountryTwoLetterCode);
        }

        var result = await _ip2cClient.GetCountryInfoAsync(ip, cancellationToken);

        return result.Status switch
        {
            // Caught by ExceptionHandlingMiddleware and turned into a 400.
            Ip2CStatus.Invalid => throw new InvalidIpAddressException(ip),

            Ip2CStatus.Unknown => null,

            Ip2CStatus.Success => await PersistAndCacheAsync(ip, result, cacheKey, cancellationToken),

            _ => throw new InvalidOperationException($"Unexpected IP2C status: {result.Status}")
        };
    }

    // NOTE: unlike CountryRepository, IIpRepository has no race-safe "add if not exists"
    // for two concurrent requests on the same brand-new IP - a rare race could throw
    // DbUpdateException from SaveChangesAsync. Worth a follow-up (a GetOrAddAsync mirroring
    // CountryRepository's), not a blocker for Task 1 today.
    private async Task<IpInfoResponse> PersistAndCacheAsync(
        string ip,
        Ip2CResult result,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        var country = new Country
        {
            TwoLetterCode = result.TwoLetterCode!,
            ThreeLetterCode = result.ThreeLetterCode!,
            CountryName = result.CountryName!
        };
        await _countryRepository.AddIfNotExistsAsync(country, cancellationToken);

        var ipEntity = new Ip
        {
            Address = ip,
            CountryTwoLetterCode = country.TwoLetterCode,
            LastUpdated = DateTime.UtcNow
        };
        await _ipRepository.AddAsync(ipEntity, cancellationToken);
        await _ipRepository.SaveChangesAsync(cancellationToken);

        var response = ToResponse(country);
        await _cache.SetAsync(cacheKey, response, cancellationToken: cancellationToken);
        return response;
    }

    private static IpInfoResponse ToResponse(Country country)
        => new(country.CountryName, country.TwoLetterCode, country.ThreeLetterCode);
}
