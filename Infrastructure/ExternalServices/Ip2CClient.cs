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
        // GetAsync (not GetStringAsync) on purpose: GetStringAsync calls EnsureSuccessStatusCode
        // internally and throws on any non-2xx response. The brief's contract assumes IP2C
        // always replies 200 with a "status;..." body, even for invalid input - but a real
        // third-party service can still answer with an actual HTTP error (404, 500, etc.),
        // and that shouldn't crash the request into an unhandled 500.
        using var response = await _httpClient.GetAsync(ip, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Δεν έχουμε ένα "status;..." σώμα να διαβάσουμε σε αυτή την περίπτωση, οπότε
            // δεν μπορούμε να ξέρουμε αν η IP ήταν έγκυρη ή όχι. Invalid -> 400 είναι πιο
            // ασφαλές λάθος να δείξουμε στον caller από ένα ακατανόητο 500.
            return new Ip2CResult(Ip2CStatus.Invalid, null, null, null);
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

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
