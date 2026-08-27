using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
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

        public string AggregatedCss { get; private set; } = string.Empty;

        public void Initialize()
        {
            var cssBuilder = new StringBuilder();

            foreach (var plugin in _pluginLoader.DiscoverPlugins())
            {
                _plugins.Add(plugin);

                var assembly = plugin.GetType().Assembly;
                foreach (var resourceName in assembly.GetManifestResourceNames().Where(r => r.EndsWith(".css", StringComparison.OrdinalIgnoreCase)))
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        cssBuilder.AppendLine($"/* Loaded from {assembly.GetName().Name} -> {resourceName} */");
                        cssBuilder.AppendLine(reader.ReadToEnd());
                    }
                }
            }

            AggregatedCss = cssBuilder.ToString();
        }

    }
}
