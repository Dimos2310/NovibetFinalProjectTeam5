using Application.Abstractions;
using Application.Interfaces;
using Infrastructure.Cache;
using Infrastructure.Configuration;
using Application.Interfaces;
using Infrastructure.ExternalServices;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.BackgroundJobs;

namespace Infrastructure;

// This is where the Infrastructure project registers everything it provides, so
// Program.cs just calls AddInfrastructure(...) once instead of listing every service by hand.
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Prefer whatever's in appsettings.json; fall back to the hardcoded one only if
        // that's somehow missing (shouldn't normally happen, just a safety net).
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? DbConnection.ConnectionString;

        // This is what actually lets AppDbContext be injected anywhere via its constructor.
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        // Whenever something asks for ICountryRepository/IIpRepository, give it the real
        // EF Core versions. This is the one line that connects the interfaces to their
        // actual implementation - nothing else in the app needs to know EF exists.
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IIpRepository, IpRepository>();

<<<<<<< HEAD
        // Binds each settings section from appsettings.json to its options class, so
        // whoever builds the cache/job/IP2C client can inject IOptions<T> instead of
        // hardcoding these values or reading IConfiguration directly.
        services.Configure<Ip2COptions>(configuration.GetSection(Ip2COptions.SectionName));
        services.Configure<CacheOptions>(configuration.GetSection(CacheOptions.SectionName));
        services.Configure<IpUpdateJobOptions>(configuration.GetSection(IpUpdateJobOptions.SectionName));

        // TODO (Infrastructure dev): register IIp2CClient, ICacheService and the
        // IP update BackgroundService here once those pieces land.
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();


        services.AddHttpClient<IIp2CClient, Ip2CClient>(client =>
        {
            client.BaseAddress = new Uri("https://ip2c.org/");
        });
        //check if ok
=======
        // Task 2: εγγράφει options + IIpRefreshService + τον hourly BackgroundService.
        services.AddIpRefreshJob(configuration);

>>>>>>> origin/LeonardoVreka
        return services;
    }
}
