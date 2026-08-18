using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Secora.Abstractions;
using Secora.Core.PluginLoader;

namespace Secora.Core.PluginManager
{
    public class PluginManager : IPluginManager
    {
        private readonly IPluginLoader _pluginLoader;
        public PluginManager(IPluginLoader pluginLoader)
        {

            _pluginLoader = pluginLoader;
            Initialize();

        }
        private readonly List<ISecoraPlugin> _plugins = new();

        public IReadOnlyCollection<ISecoraPlugin> Plugins => _plugins;

        public void Initialize()
        {
            foreach (var plugin in _pluginLoader.DiscoverPlugins())
            {
                _plugins.Add(plugin);
            }
        }

    }
}
