using Application.BackgroundJobs;      // IpRefreshOptions, IIpRefreshService, IpRefreshService
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Εγγραφή όλου του IP refresh job (Task 2) με μία κλήση: δένει τα settings του,
/// καταχωρεί τη λογική ανανέωσης, και ξεκινά τον ωριαίο scheduler.
/// </summary>
// Static class - this only ever calls static registration methods, never needs an instance.
public static class IpRefreshJobRegistration
{
    // Extension method, so callers write services.AddIpRefreshJob(configuration) - same
    // pattern as AddDbContext, AddControllers, etc.
    public static IServiceCollection AddIpRefreshJob(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Δένει το section "IpUpdateJob" του appsettings μέσα στο IpRefreshOptions.
        services.Configure<IpRefreshOptions>(
            configuration.GetSection(IpRefreshOptions.SectionName));

        // 2. Καταχωρεί τη λογική. Scoped, ώστε κάθε εκτέλεση να παίρνει φρέσκο αντικείμενο
        //    (με φρέσκο repository/DbContext) μέσα στο scope του background service.
        services.AddScoped<IIpRefreshService, IpRefreshService>();

        // 3. Ξεκινά τον ωριαίο scheduler ως hosted background service.
        services.AddHostedService<IpRefreshBackgroundService>();

        return services;
    }
}
