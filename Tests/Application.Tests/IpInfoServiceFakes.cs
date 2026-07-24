using Application.Abstractions;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Tests;

// In-memory υποκατάστατο του ICacheService - πραγματικό get/set/remove χωρίς IMemoryCache,
// έτσι ώστε το IpInfoService να ελέγχεται χωρίς Infrastructure. Μετράει επίσης τις κλήσεις,
// ώστε ένα test να μπορεί να επιβεβαιώσει ότι πραγματικά χτυπήθηκε (ή όχι) η cache.
internal sealed class FakeCacheService : ICacheService
{
    private readonly Dictionary<string, object?> _store = new();

    public int GetCallCount { get; private set; }
    public int SetCallCount { get; private set; }
    public int RemoveCallCount { get; private set; }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        GetCallCount++;
        return Task.FromResult(_store.TryGetValue(key, out var value) ? (T?)value : default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default)
    {
        SetCallCount++;
        _store[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        RemoveCallCount++;
        _store.Remove(key);
        return Task.CompletedTask;
    }
}

// In-memory υποκατάστατο του ICountryRepository. Ξεχωριστό από ένα τυχόν fake στα
// Infrastructure.Tests - εδώ ελέγχουμε μόνο τη λογική του IpInfoService, όχι EF Core.
internal sealed class FakeCountryRepository : ICountryRepository
{
    private readonly Dictionary<string, Country> _countries = new();

    public int AddIfNotExistsCallCount { get; private set; }

    public FakeCountryRepository(params Country[] seed)
    {
        foreach (var country in seed)
        {
            _countries[country.TwoLetterCode] = country;
        }
    }

    public Task<Country?> GetByCodeAsync(string twoLetterCode, CancellationToken ct = default)
        => Task.FromResult(_countries.TryGetValue(twoLetterCode, out var country) ? country : null);

    public Task AddIfNotExistsAsync(Country country, CancellationToken ct = default)
    {
        AddIfNotExistsCallCount++;
        if (!_countries.ContainsKey(country.TwoLetterCode))
        {
            _countries[country.TwoLetterCode] = country;
        }
        return Task.CompletedTask;
    }
}

// In-memory υποκατάστατο του IIpRepository που πραγματικά υποστηρίζει το μονοπάτι που
// χρησιμοποιεί το IpInfoService (GetByAddressAsync / AddAsync / SaveChangesAsync) - σε
// αντίθεση με το FakeIpRepository του ReportServiceTests, που τα πετάει ως NotSupported
// επειδή το ReportService δεν τα χρειάζεται καθόλου.
internal sealed class InMemoryIpRepository : IIpRepository
{
    private readonly Dictionary<string, Ip> _ips = new();
    private readonly List<Ip> _pendingAdds = new();

    public int SaveChangesCallCount { get; private set; }

    public InMemoryIpRepository(params Ip[] seed)
    {
        foreach (var ip in seed)
        {
            _ips[ip.Address] = ip;
        }
    }

    public Task<Ip?> GetByAddressAsync(string address, CancellationToken ct = default)
        => Task.FromResult(_ips.TryGetValue(address, out var ip) ? ip : null);

    public Task<Ip> AddAsync(Ip ip, CancellationToken ct = default)
    {
        _pendingAdds.Add(ip);
        return Task.FromResult(ip);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveChangesCallCount++;
        foreach (var ip in _pendingAdds)
        {
            _ips[ip.Address] = ip;
        }
        _pendingAdds.Clear();
        return Task.CompletedTask;
    }

    // Δεν χρησιμοποιούνται από το IpInfoService - υπάρχουν μόνο για να ικανοποιηθεί το interface.
    public Task<IReadOnlyList<Ip>> GetBatchAsync(int skip, int take, CancellationToken ct = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<CountryReportItem>> GetReportAsync(string[]? countryCodes, CancellationToken ct = default)
        => throw new NotSupportedException();
}

// In-memory υποκατάστατο του IIp2CClient - επιστρέφει πάντα το απαντητικό που όρισε το
// test, και θυμάται με ποια IP κλήθηκε, ώστε ένα test να μπορεί να το επιβεβαιώσει.
internal sealed class FakeIp2CClient : IIp2CClient
{
    private readonly Ip2CResult _result;

    public string? LastRequestedIp { get; private set; }
    public int CallCount { get; private set; }

    public FakeIp2CClient(Ip2CResult result) => _result = result;

    public Task<Ip2CResult> GetCountryInfoAsync(string ip, CancellationToken cancellationToken = default)
    {
        LastRequestedIp = ip;
        CallCount++;
        return Task.FromResult(_result);
    }
}
