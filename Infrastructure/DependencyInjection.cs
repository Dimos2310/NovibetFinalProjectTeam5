using Application.Abstractions;
using Application.Interfaces;
using Infrastructure.Cache;
using Infrastructure.Configuration;
using Infrastructure.ExternalServices;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Infrastructure.BackgroundJobs;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? DbConnection.ConnectionString;

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IIpRepository, IpRepository>();

        services.Configure<Ip2COptions>(configuration.GetSection(Ip2COptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        services.AddHttpClient<IIp2CClient, Ip2CClient>((provider, client) =>
        {
            var ip2cOptions = provider.GetRequiredService<IOptions<Ip2COptions>>().Value;
            client.BaseAddress = new Uri(ip2cOptions.BaseUrl);
        });

        services.AddIpRefreshJob(configuration);

        return services;
    }
}
