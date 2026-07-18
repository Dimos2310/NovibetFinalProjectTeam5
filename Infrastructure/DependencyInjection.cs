using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Composition entry point for the Infrastructure layer.
///
/// NOTE (P1): this is an intentional skeleton so the solution compiles and runs
/// end-to-end from day one. The Infrastructure developer fills it in with the
/// DbContext, repositories, IP2C HTTP client, cache and the hourly background job.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // TODO (Infrastructure dev): register DbContext, repositories,
        // IIp2CClient, ICacheService and the IP update BackgroundService here.
        return services;
    }
}
