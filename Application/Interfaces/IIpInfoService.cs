using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Use-case contract for the "IP information" endpoint (Task 1). The implementation
/// performs the cache → database → IP2C fallback and back-fills both stores.
/// </summary>
public interface IIpInfoService
{
    /// <summary>
    /// Returns the country details for the given IP, or null if the IP is unknown/invalid.
    /// </summary>
    Task<IpInfoResponse?> GetIpInfoAsync(string ip, CancellationToken cancellationToken = default);
}
