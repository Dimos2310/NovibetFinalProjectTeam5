using Application.BackgroundJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.BackgroundJobs;

public static class IpRefreshJobRegistration
{
    public static IServiceCollection AddIpRefreshJob(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IpRefreshOptions>(
            configuration.GetSection(IpRefreshOptions.SectionName));

        services.AddScoped<IIpRefreshService, IpRefreshService>();
        services.AddHostedService<IpRefreshBackgroundService>();

        return services;
    }
}
