using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Σημείο σύνθεσης για το Application layer. Κάθε layer εκθέτει τη δική του μέθοδο
/// registration, ώστε το Program.cs του API να μένει λιτό και η ομάδα να αποφεύγει
/// merge conflicts πάνω σε ένα κοινό αρχείο wiring.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Task 3 - report endpoint. Scoped επειδή εξαρτάται από το IIpRepository, το οποίο
        // είναι scoped (τυλίγει το DbContext που ζει όσο το κάθε request).
        services.AddScoped<IReportService, ReportService>();

        // Task 1 - IP lookup endpoint (cache -> βάση -> IP2C). Ίδιος λόγος για Scoped:
        // εξαρτάται από τα ίδια scoped repositories.
        services.AddScoped<IIpInfoService, IpInfoService>();

        return services;
    }
}
