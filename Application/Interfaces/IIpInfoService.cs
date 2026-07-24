using Application.DTOs;

namespace Application.Interfaces;

public interface IIpInfoService
{
    // Null means the IP is valid but its country is unknown (IP2C status 2).
    Task<IpInfoResponse?> GetIpInfoAsync(string ip, CancellationToken cancellationToken = default);
}
