using Application.DTOs;

namespace Application.Interfaces;

public interface IIp2CClient
{
    Task<Ip2CResult> GetCountryInfoAsync(string ip, CancellationToken cancellationToken = default);
}
