using Application.Abstractions;
using Application.Caching;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.BackgroundJobs;

public sealed class IpRefreshService : IIpRefreshService
{
    private readonly IIpRepository _repository;
    private readonly IIp2CClient _ip2cClient;
    private readonly ICacheService _cache;
    private readonly ILogger<IpRefreshService> _logger;
    private readonly int _batchSize;

    public IpRefreshService(
        IIpRepository repository,
        IIp2CClient ip2cClient,
        ICacheService cache,
        IOptions<IpRefreshOptions> options,
        ILogger<IpRefreshService> logger)
    {
        _repository = repository;
        _ip2cClient = ip2cClient;
        _cache = cache;
        _logger = logger;

        var configured = options.Value.BatchSize;
        _batchSize = configured > 0 ? configured : 100;
    }

    public async Task RefreshAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("IP refresh started (batch size {BatchSize}).", _batchSize);

        var skip = 0;
        var totalChanged = 0;

        // Pages through the table so the whole thing is never loaded into memory at once.
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var batch = await _repository.GetBatchAsync(skip, _batchSize, cancellationToken);
            if (batch.Count == 0)
                break;

            totalChanged += await RefreshBatchAsync(batch, cancellationToken);
            skip += batch.Count;

            if (batch.Count < _batchSize)
                break;
        }

        _logger.LogInformation("IP refresh finished. {Changed} address(es) updated.", totalChanged);
    }

    private async Task<int> RefreshBatchAsync(IReadOnlyList<Ip> batch, CancellationToken cancellationToken)
    {
        var changed = new List<Ip>();

        foreach (var ip in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await _ip2cClient.GetCountryInfoAsync(ip.Address, cancellationToken);

                if (!result.IsSuccess || result.TwoLetterCode is null)
                {
                    _logger.LogDebug("Skipping {Address}: IP2C status {Status}.", ip.Address, result.Status);
                    continue;
                }

                if (string.Equals(result.TwoLetterCode, ip.CountryTwoLetterCode, StringComparison.OrdinalIgnoreCase))
                    continue;

                ip.CountryTwoLetterCode = result.TwoLetterCode;
                ip.LastUpdated = DateTime.UtcNow;
                changed.Add(ip);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw; // app is shutting down - stop the whole run promptly
            }
            catch (Exception ex)
            {
                // One IP's failure must not sink the batch.
                _logger.LogWarning(ex, "Failed to refresh {Address}; continuing with the rest.", ip.Address);
            }
        }

        if (changed.Count == 0)
            return 0;

        await _repository.SaveChangesAsync(cancellationToken);

        foreach (var ip in changed)
        {
            // Shared with Task 1 via CacheKeys.ForIp so the key can never drift.
            await _cache.RemoveAsync(CacheKeys.ForIp(ip.Address), cancellationToken);
        }

        return changed.Count;
    }
}
