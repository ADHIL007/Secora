using Microsoft.Extensions.DependencyInjection;
using Secora.Abstractions;
using Secora.Core.PluginLoader;
using Secora.Core.PluginManager;
using Secora.AspNetCore.Extensions;

namespace Secora.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSecora(this IServiceCollection services)
    {
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton<IPluginManager, PluginManager>();

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
