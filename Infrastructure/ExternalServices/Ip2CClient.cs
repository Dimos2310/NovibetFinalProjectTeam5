using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;

namespace Infrastructure.ExternalServices;

public class Ip2CClient : IIp2CClient
{
    private readonly HttpClient _httpClient;

    public Ip2CClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Ip2CResult> GetCountryInfoAsync(string ip, CancellationToken cancellationToken = default)
    {
        // GetAsync, not GetStringAsync - the latter throws on any non-2xx response, and a
        // real third-party service can answer with an actual HTTP error, not just a
        // "status;..." body. Treat that as Invalid instead of crashing the request.
        using var response = await _httpClient.GetAsync(ip, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new Ip2CResult(Ip2CStatus.Invalid, null, null, null);
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParseResponse(raw);
    }

    private static Ip2CResult ParseResponse(string raw)
    {
        var parts = raw.Split(';');
        var status = (Ip2CStatus)int.Parse(parts[0]);

        if (status != Ip2CStatus.Success)
        {
            return new Ip2CResult(status, null, null, null);
        }
        return new Ip2CResult(status, parts[1], parts[2], parts[3]);
    }
}
