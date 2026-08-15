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
        services.AddTransient<IPluginLoader, PluginLoader>();
        services.AddSingleton<IPluginManager, PluginManager>();

        services.AddRazorComponents()
                  .AddInteractiveServerComponents();

        return services;
    }
}