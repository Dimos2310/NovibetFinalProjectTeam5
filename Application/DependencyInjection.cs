using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Composition entry point for the Application layer. Each layer exposes its own
/// registration method so the API's Program.cs stays thin and the team avoids
/// merge conflicts on a single wiring file.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Concrete use-case services (IIpInfoService, IReportService) are registered here
        // by the endpoint developers once their implementations land.
        return services;
    }
}
