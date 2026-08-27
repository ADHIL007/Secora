using Microsoft.Extensions.DependencyInjection;
using Secora.Abstractions;
using Secora.Core.PluginLoader;
using Secora.Core.PluginManager;
using Secora.Core;

namespace Secora.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Secora services with default options.
    /// </summary>
    public static IServiceCollection AddSecora(this IServiceCollection services)
    {
        return services.AddSecora(_ => { });
    }

    /// <summary>
    /// Registers Secora services with user-configurable options.
    /// <code>
    /// builder.Services.AddSecora(options =>
    /// {
    ///     options.RoutePrefix = "/security-dashboard";
    ///     options.HealthCheckPrefixes.Add("/my-custom-health");
    ///     options.AdditionalInfrastructurePrefixes.Add("/internal-admin");
    /// });
    /// </code>
    /// </summary>
    public static IServiceCollection AddSecora(
        this IServiceCollection services,
        Action<SecoraOptions> configure)
    {
        // Register and configure options
        services.Configure(configure);

        // Core services
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton<IPluginManager, PluginManager>();
        services.AddSingleton<EndpointClassifier>();
        services.AddSingleton<EndPointScanner>();

        services.AddCors(options =>
        {
            options.AddPolicy("SecoraPolicy", policy =>
            {
                policy.SetIsOriginAllowed(_ => true)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        services.AddRazorComponents()
                  .AddInteractiveServerComponents();

        return services;
    }
}
