using Application.Abstractions;
using Application.DTOs;
using Domain.Entities;

namespace Application.Tests;

internal sealed class FakeIpRepository : IIpRepository
{
    private readonly IReadOnlyList<CountryReportItem> _rows;

    public FakeIpRepository(params CountryReportItem[] rows) => _rows = rows;

    // Null is a meaningful value here (= "all countries"), so WasCalled tracks
    // separately whether GetReportAsync was actually invoked.
    public string[]? ReceivedCodes { get; private set; }
    public bool WasCalled { get; private set; }

    public Task<IReadOnlyList<CountryReportItem>> GetReportAsync(
        string[]? countryCodes,
        CancellationToken ct = default)
    {
        ReceivedCodes = countryCodes;
        WasCalled = true;
        return Task.FromResult(_rows);
    }

    public Task<Ip?> GetByAddressAsync(string address, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<Ip> AddAsync(Ip ip, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task SaveChangesAsync(CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<Ip>> GetBatchAsync(int skip, int take, CancellationToken ct = default)
        => throw new NotSupportedException();
}
