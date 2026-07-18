using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Abstraction over the external IP2C web service. Keeps the application logic free of
/// any knowledge about HTTP or the ip2c.org host, and lets tests supply a fake.
/// </summary>
public interface IIp2CClient
{
    /// <summary>
    /// Calls IP2C for the given IP and returns the parsed result
    /// (status + country codes + name).
    /// </summary>
    Task<Ip2CResult> GetCountryInfoAsync(string ip, CancellationToken cancellationToken = default);
}
