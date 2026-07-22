using Domain.Enums;
using Infrastructure.ExternalServices;
using Xunit;

namespace Infrastructure.Tests;

public class Ip2CClientTests
{
    private static (Ip2CClient Client, FakeHttpMessageHandler Handler) CreateClient(string fakeResponseBody)
    {
        var handler = new FakeHttpMessageHandler(fakeResponseBody);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://ip2c.org/") };
        return (new Ip2CClient(httpClient), handler);
    }

    // Verifies that a successful IP2C response (status 1) is parsed into the correct country codes and name.
    [Fact]
    public async Task GetCountryInfoAsync_parses_a_successful_response()
    {
        var (client, _) = CreateClient("1;GR;GRC;Greece");

        var result = await client.GetCountryInfoAsync("8.8.8.8");

        Assert.Equal(Ip2CStatus.Success, result.Status);
        Assert.True(result.IsSuccess);
        Assert.Equal("GR", result.TwoLetterCode);
        Assert.Equal("GRC", result.ThreeLetterCode);
        Assert.Equal("Greece", result.CountryName);
    }

    // Verifies that an invalid-IP response (status 0) returns null country info instead of throwing or misreading empty fields.
    [Fact]
    public async Task GetCountryInfoAsync_returns_nulls_for_an_invalid_ip()
    {
        var (client, _) = CreateClient("0;;;");

        var result = await client.GetCountryInfoAsync("not-an-ip");

        Assert.Equal(Ip2CStatus.Invalid, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Null(result.TwoLetterCode);
        Assert.Null(result.ThreeLetterCode);
        Assert.Null(result.CountryName);
    }

    // Verifies that an unknown-country response (status 2) also returns null country info, not just the invalid case.
    [Fact]
    public async Task GetCountryInfoAsync_returns_nulls_for_an_unknown_country()
    {
        var (client, _) = CreateClient("2;;;");

        var result = await client.GetCountryInfoAsync("203.0.113.1");

        Assert.Equal(Ip2CStatus.Unknown, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Null(result.TwoLetterCode);
    }

    // Verifies that the IP address is sent correctly as part of the outgoing request URL to ip2c.org.
    [Fact]
    public async Task GetCountryInfoAsync_sends_the_ip_as_the_request_uri()
    {
        var (client, handler) = CreateClient("1;US;USA;United States");

        await client.GetCountryInfoAsync("1.2.3.4");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://ip2c.org/1.2.3.4", handler.LastRequest!.RequestUri!.ToString());
    }

    // Καλύπτει ένα πραγματικό bug που βρέθηκε ζωντανά: αν στείλουμε κάτι που δεν είναι
    // πραγματική IP (π.χ. "GR"), το ip2c.org μπορεί να απαντήσει με πραγματικό HTTP 404
    // αντί για το αναμενόμενο 200 με "0;;;". Πριν το fix, το GetStringAsync πετούσε
    // HttpRequestException που έφτανε ασυγκράτητο μέχρι το middleware ως 500. Αυτό
    // επιβεβαιώνει ότι τώρα το χειριζόμαστε σαν Invalid, όπως ένα κανονικό status=0.
    [Fact]
    public async Task GetCountryInfoAsync_treats_a_non_success_http_response_as_invalid()
    {
        var handler = new FakeHttpMessageHandler(string.Empty, System.Net.HttpStatusCode.NotFound);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://ip2c.org/") };
        var client = new Ip2CClient(httpClient);

        var result = await client.GetCountryInfoAsync("GR");

        Assert.Equal(Ip2CStatus.Invalid, result.Status);
        Assert.False(result.IsSuccess);
        Assert.Null(result.TwoLetterCode);
    }
}
