using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;

namespace Infrastructure.ExternalServices;

  public class Ip2CClient : IIp2CClient
{
    private readonly HttpClient _httpClient; // type for dotnet

    public Ip2CClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Ip2CResult> GetCountryInfoAsync(string ip,CancellationToken cancellationToken = default)
    {
        var raw = await _httpClient.GetStringAsync(ip,cancellationToken);

        return ParseResponse(raw); //helper method to parse the raw response into a structured Ip2CResult
    }

    private static Ip2CResult ParseResponse(string raw)
    {
        var parts = raw.Split(';'); //we split the string into a array of strings
        var status =(Ip2CStatus)int.Parse(parts[0]); //string to int and then Ip2CStatus enum

        if (status != Ip2CStatus.Success)
        {
            return new Ip2CResult(status, null, null, null); //if not success return the status and nulls no need for extra fields
        }
        return new Ip2CResult(status, parts[1], parts[2], parts[3]); //if success return the status and the other fieldsa
    }
}
