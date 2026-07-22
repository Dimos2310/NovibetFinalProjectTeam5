using Application.Abstractions;
using Application.Caching;
using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Application.Services;

// Task 1: "cache πρώτα, μετά η βάση, και τέλος το IP2C - με persist + cache στο τέλος".
// Κάθε βήμα κοστίζει περισσότερο από το προηγούμενο (μνήμη -> SQL -> δίκτυο προς τρίτους),
// γι' αυτό ελέγχουμε με αυτή τη σειρά και σταματάμε μόλις βρούμε απάντηση.
public class IpInfoService : IIpInfoService
{
    private readonly ICacheService _cache;
    private readonly ICountryRepository _countryRepository;
    private readonly IIpRepository _ipRepository;
    private readonly IIp2CClient _ip2cClient;
    private readonly ILogger<IpInfoService> _logger;

    public IpInfoService(
        ICacheService cache,
        ICountryRepository countryRepository,
        IIpRepository ipRepository,
        IIp2CClient ip2cClient,
        ILogger<IpInfoService> logger)
    {
        _cache = cache;
        _countryRepository = countryRepository;
        _ipRepository = ipRepository;
        _ip2cClient = ip2cClient;
        _logger = logger;
    }

    public async Task<IpInfoResponse?> GetIpInfoAsync(string ip, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeys.ForIp(ip);

        // (1) Cache πρώτα - το φθηνότερο και γρηγορότερο βήμα.
        var cached = await _cache.GetAsync<IpInfoResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        // (2) Μετά η βάση - φθηνότερο από το να ρωτήσουμε το εξωτερικό IP2C.
        var existingIp = await _ipRepository.GetByAddressAsync(ip, cancellationToken);
        if (existingIp is not null)
        {
            var storedCountry = await _countryRepository.GetByCodeAsync(
                existingIp.CountryTwoLetterCode, cancellationToken);

            if (storedCountry is not null)
            {
                var response = ToResponse(storedCountry);
                await _cache.SetAsync(cacheKey, response, cancellationToken: cancellationToken);
                return response;
            }

            // Δεν θα έπρεπε ποτέ να συμβεί - το foreign key στο AppDbContext το εγγυάται -
            // αλλά αν κάποιος πείραξε χειροκίνητα τη βάση, δεν γκρεμίζουμε το request εδώ.
            // Συνεχίζουμε στο IP2C σαν να μην είχαμε βρει τίποτα, ώστε το request να
            // αυτοθεραπεύσει το πρόβλημα αντί να επιστρέψει σφάλμα στον caller.
            _logger.LogWarning(
                "Ip {Address} referenced country {Code} which no longer exists; re-resolving via IP2C.",
                ip, existingIp.CountryTwoLetterCode);
        }

        // (3) Τελευταία λύση - το εξωτερικό service. Έχει πραγματικό κόστος (δίκτυο,
        // πιθανό rate limit στο ip2c.org), γι' αυτό φτάνουμε εδώ μόνο σε cache+DB miss.
        var result = await _ip2cClient.GetCountryInfoAsync(ip, cancellationToken);

        return result.Status switch
        {
            // status = 0 στο IP2C: η ExceptionHandlingMiddleware πιάνει αυτό το exception
            // πιο πάνω στο pipeline και το μετατρέπει σε 400 - δεν το πιάνουμε εδώ επίτηδες.
            Ip2CStatus.Invalid => throw new InvalidIpAddressException(ip),

            // status = 2: έγκυρη IP, αλλά χωρίς γνωστή χώρα. Δεν έχουμε τι να αποθηκεύσουμε
            // (το CountryTwoLetterCode είναι υποχρεωτικό πεδίο στο Ip), οπότε null - ο
            // caller (IpController) το μεταφράζει σε 404, όχι σε σφάλμα.
            Ip2CStatus.Unknown => null,

            // status = 1: η μόνη περίπτωση που έχουμε πραγματικά κάτι να αποθηκεύσουμε.
            Ip2CStatus.Success => await PersistAndCacheAsync(ip, result, cacheKey, cancellationToken),

            // Ασφάλεια αν το IP2C επιστρέψει ποτέ κάτι εκτός των 0/1/2 που περιγράφει η εκφώνηση.
            _ => throw new InvalidOperationException($"Unexpected IP2C status: {result.Status}")
        };
    }

    // (4) Επιτυχία από το IP2C: εξασφαλίζουμε τη χώρα (idempotent, ασφαλές σε race μεταξύ
    // ταυτόχρονων requests - βλ. CountryRepository.AddIfNotExistsAsync), αποθηκεύουμε το
    // νέο Ip, γεμίζουμε την cache, και επιστρέφουμε.
    //
    // ΣΗΜΕΙΩΣΗ: σε αντίθεση με το CountryRepository, το IIpRepository δεν έχει ακόμα
    // αντίστοιχο προστατευμένο "add if not exists" για ταυτόχρονα requests πάνω στην ίδια
    // ολοκαίνουργια IP (unique index σε Address). Σε πολύ σπάνια περίπτωση race, το
    // SaveChangesAsync παρακάτω θα πετάξει DbUpdateException από το EF Core, που δεν την
    // πιάνουμε εδώ επίτηδες (το Application layer δεν πρέπει να ξέρει τίποτα για EF) -
    // ιδανικά θα λυνόταν με το IIpRepository να εκθέτει κάτι σαν GetOrAddAsync, αντίστοιχο
    // με του CountryRepository. Αξίζει follow-up, όχι μπλοκάρισμα του Task 1 τώρα.
    private async Task<IpInfoResponse> PersistAndCacheAsync(
        string ip,
        Ip2CResult result,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        var country = new Country
        {
            TwoLetterCode = result.TwoLetterCode!,
            ThreeLetterCode = result.ThreeLetterCode!,
            CountryName = result.CountryName!
        };
        await _countryRepository.AddIfNotExistsAsync(country, cancellationToken);

        var ipEntity = new Ip
        {
            Address = ip,
            CountryTwoLetterCode = country.TwoLetterCode,
            LastUpdated = DateTime.UtcNow
        };
        await _ipRepository.AddAsync(ipEntity, cancellationToken);
        await _ipRepository.SaveChangesAsync(cancellationToken);

        var response = ToResponse(country);
        await _cache.SetAsync(cacheKey, response, cancellationToken: cancellationToken);
        return response;
    }

    private static IpInfoResponse ToResponse(Country country)
        => new(country.CountryName, country.TwoLetterCode, country.ThreeLetterCode);
}
