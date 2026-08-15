using Microsoft.Extensions.DependencyInjection;
using Secora.Abstractions;
using Secora.Core.PluginLoader;
using Secora.Core.PluginManager;

namespace Secora.AspNetCore.Extensions
{
    public class ServiceCollectionExtensions
    {
        public void AddSecora(IServiceCollection services)
        {

            services.AddTransient<IPluginLoader, PluginLoader>();
            services.AddSingleton<IPluginManager, PluginManager>();

        }

    }
}
