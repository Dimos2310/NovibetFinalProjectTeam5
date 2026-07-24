<<<<<<< HEAD
using Application.BackgroundJobs;      // IpRefreshOptions, IIpRefreshService, IpRefreshService
=======
﻿using Application.BackgroundJobs;      // IpRefreshOptions, IIpRefreshService, IpRefreshService
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Εγγραφή όλου του IP refresh job (Task 2) με μία κλήση: δένει τα settings του,
/// καταχωρεί τη λογική ανανέωσης, και ξεκινά τον ωριαίο scheduler.
/// </summary>
<<<<<<< HEAD
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
=======

// static class gt den xreiazetai na ftiaxnei antikeimeno apo panw, mono na kalesi tis static methods
public static class IpRefreshJobRegistration
{
    // static method
    public static IServiceCollection AddIpRefreshJob(
        this IServiceCollection services,                  // extension method gia na kalesi apo panw me to services.AddIpRefreshJob(configuration)
        IConfiguration configuration)                      // gia na parei ta settings apo to configuration (appsettings.json)
    {
        // 1. Δένει το section "IpRefresh" του appsettings μέσα στο IpRefreshOptions.
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
        services.Configure<IpRefreshOptions>(
            configuration.GetSection(IpRefreshOptions.SectionName));

        // 2. Καταχωρεί τη λογική. Scoped, ώστε κάθε εκτέλεση να παίρνει φρέσκο αντικείμενο
        //    (με φρέσκο repository/DbContext) μέσα στο scope του background service.
        services.AddScoped<IIpRefreshService, IpRefreshService>();

        // 3. Ξεκινά τον ωριαίο scheduler ως hosted background service.
        services.AddHostedService<IpRefreshBackgroundService>();

        return services;
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> 636d28f64219dcc9db3298d93335691298663db5
