using Application.Abstractions;   // IIpRepository
using Application.DTOs;           // Ip2CResult
using Application.Interfaces;     // IIp2CClient, ICacheService
using Domain.Entities;            // Ip
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.BackgroundJobs;

/// <summary>
/// Refreshes every stored IP against IP2C and keeps the database and cache in sync.
/// Reads the database one page at a time, writes/invalidates only when an IP's country
/// actually changed, and never lets a single failed lookup abort the whole run.
/// </summary>
public sealed class IpRefreshService : IIpRefreshService /// sealed gt kanenas den prepei na kanei iherit to service
{
    //parametroi ti xreiazomai
    private readonly IIpRepository _repository;           // h vasi dedomenwn ip pou exw sto db
    private readonly IIp2CClient _ip2cClient;             // rwtaei to ip2c gia na gurnaei to country code
    private readonly ICacheService _cache;                // gia na svisei palies egrafes apo tin cache otan kati allaksei
    private readonly ILogger<IpRefreshService> _logger;  // gia log messages, errors, warnings
    private readonly int _batchSize;                     // to posa ip tha diavazw kai tha elegxw se kathe batch

    //constructor pou pairnei ta dependencies apo to DI container kai ta apothikeuei se private fields
    public IpRefreshService(
        // ta pairnei apo panw
        IIpRepository repository,
        IIp2CClient ip2cClient,
        ICacheService cache,
        IOptions<IpRefreshOptions> options,
        ILogger<IpRefreshService> logger)
    {
        // ta apothikeuei se private fields gia na ta xrisimopoiei meta
        _repository = repository;
        _ip2cClient = ip2cClient;
        _cache = cache;
        _logger = logger;

        // ta apothikeuw alla ta douleuw ligo prwta, px pairnw to perixomeno tou options.Value.BatchSize kai to apothikeuw sto _batchSize 
        var configured = options.Value.BatchSize;
        _batchSize = configured > 0 ? configured : 100;
    }

    //methods
    public async Task RefreshAllAsync(CancellationToken cancellationToken = default)
    {
        // prwto log message gia na doume oti to refresh arxise
        _logger.LogInformation("IP refresh started (batch size {BatchSize}).", _batchSize);

        // metavlites to skip m leei pou vriskomai dld oti exw parei idi ta prwta 100 kai na kserei pou vrisketai kai to allo posa exoun allasksei sunolika
        var skip = 0;
        var totalChanged = 0;

        // (1) Page through the table so we never load the whole thing into memory.
        while (true)
        {
            //koumpi akuroseis gia na stamatisei to refresh an to cancellationToken exei zhtisei akyrwsi
            cancellationToken.ThrowIfCancellationRequested();

            //fernei ta epomena batchSize ip apo tin vasi dedomenwn, me skip kai take gia na parw ta epomena batchSize an ginei 0 tote exoume ftasei sto telos kai kanoume break
            var batch = await _repository.GetBatchAsync(skip, _batchSize, cancellationToken);
            if (batch.Count == 0)
                break;
            // kaneis +1 sto totalChanged gia na doume posa allaksan
            totalChanged += await RefreshBatchAsync(batch, cancellationToken);
            // allzei tin timi tou wste na kseroume posa doulepsame kai na sunexisoume me ta epomena batchSize
            skip += batch.Count;

            // otan to batch pou fernei einai mikrotero apo to batchSize tote exoume ftasei sto telos kai kanoume break gia na min sinexisei askopa se epomeno batch size me 0 ip
            if (batch.Count < _batchSize)
                break;
        }
        // log message gia na doume oti to refresh teleiwse kai posa allaskan
        _logger.LogInformation("IP refresh finished. {Changed} address(es) updated.", totalChanged);
    }

    // pairnei mia partida me 100 ip kai elegxei kathe ena an exei allaksei to country code kai an nai to apothikeuei sto db kai svinei tin palia egrafi apo tin cache
    private async Task<int> RefreshBatchAsync(IReadOnlyList<Ip> batch, CancellationToken cancellationToken)
    {
        //friaxnei mia adeia lista pou tha apothikeuei ta ip pou allaksan
        var changed = new List<Ip>();

        // elegxei kathe ip sto batch
        foreach (var ip in batch)
        {
            // elegxos akurwseis
            cancellationToken.ThrowIfCancellationRequested();


            // kanei pramata mesa sto try, an kati ginei lathos tote to catch tha to traparei kai tha logarei to lathos alla den tha stamataei to refresh gia ta alla ip
            try
            {
                // (2) stelnei to ip sto ip2c client kai pairnei to apotelesma, an to cancellationToken exei zhtisei akyrwsi tote stamataei to refresh
                var result = await _ip2cClient.GetCountryInfoAsync(ip.Address, cancellationToken);

                // (3) an to result den einai epityxes i to TwoLetterCode einai null tote logarei oti to ip2c den epityxe na gurnaei country code kai sunexizei me to epomeno ip
                if (!result.IsSuccess || result.TwoLetterCode is null)
                {
                    _logger.LogDebug("Skipping {Address}: IP2C status {Status}.", ip.Address, result.Status);
                    continue;
                }

                // (4) an to country code einai idio me to palio pou exei to ip sto db tote sunexizei me to epomeno ip
                if (string.Equals(result.TwoLetterCode, ip.CountryTwoLetterCode, StringComparison.OrdinalIgnoreCase))
                    continue;

                // (5) an ta apo panw den isxunoun tote allazei to country code tou ip sto db kai prosthetei to ip sti lista me ta allagmena ip
                ip.CountryTwoLetterCode = result.TwoLetterCode;
                ip.LastUpdated = DateTime.UtcNow;
                changed.Add(ip);
            }
            // an to cancellationToken exei zhtisei akyrwsi tote stamataei to refresh kai gurnaei sto panw method
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw; // app is shutting down — stop the whole run promptly
            }
            // an kati allo ginei lathos tote logarei to lathos kai sunexizei me to epomeno ip
            catch (Exception ex)
            {
                // One IP's failure must not sink the batch. Log it and carry on with the rest.
                _logger.LogWarning(ex, "Failed to refresh {Address}; continuing with the rest.", ip.Address);
            }
        }
        // an den exoun allaksei ta ip tote den peirazei tin cache kai gurnaei 0
        if (changed.Count == 0)
            return 0;

        // (6) kanei save tis allages tou sigkekrimenou batch sto db kai svinei tin palia egrafi apo tin cache gia kathe ip pou allaksei
        await _repository.SaveChangesAsync(cancellationToken);
        // pali gia kathe ip pou allakse svinei tin palia egrafi apo tin cache gia na min exei palia dedomena
        foreach (var ip in changed)
        {
            // ⚠ RECONCILE: this key must match the one Task 1 uses in ICacheService.SetAsync.
            // Best: both sides share a single CacheKeys.ForIp(address) helper so they can't drift.
            await _cache.RemoveAsync(ip.Address, cancellationToken);
        }
        // epistrefei posa ip allaksan
        return changed.Count;
    }
}