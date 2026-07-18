using Application.DTOs;
using Domain.Entities;

namespace Application.Abstractions;

public interface IIpRepository
{
    Task<Ip?> GetByAddressAsync(string address, CancellationToken ct = default); //returns ip used in task1 ,look at db before IP2C
    Task<Ip> AddAsync(Ip ip, CancellationToken ct = default); //adds ip to the database
    Task SaveChangesAsync(CancellationToken ct = default); //saves changes to the database
    Task<IReadOnlyList<Ip>> GetBatchAsync(int skip, int take, CancellationToken ct = default); //returns a batch of ips, the job part for the feature
    Task<IReadOnlyList<CountryReportItem>> GetReportAsync(string[]? countryCodes, CancellationToken ct = default); // report for the countries, readonly because we do not want to modify data
}