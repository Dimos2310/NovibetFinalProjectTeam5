using System.Net;

namespace Infrastructure.Tests;

// Test double for HttpMessageHandler: instead of going out over the network,
// it just returns whatever response body/status we configure in the constructor.
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _content;

    // Captured so tests can assert *what* Ip2CClient actually sent (e.g. the IP in the URL).
    public HttpRequestMessage? LastRequest { get; private set; }

    public FakeHttpMessageHandler(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _content = content;
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;

        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content)
        };
        return Task.FromResult(response);
    }
}
