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
        private readonly List<ISecoraPlugin> _plugins = new();
        private readonly Dictionary<ISecoraPlugin, string> _isolationClasses = new();
        public IReadOnlyCollection<ISecoraPlugin> Plugins => _plugins.AsReadOnly();
        public string AggregatedCss { get; private set; } = string.Empty;

        public PluginManager(IPluginLoader pluginLoader)
        {
            _pluginLoader = pluginLoader;
            Initialize();
        }

        public string GetCssIsolationClass(ISecoraPlugin plugin) =>
            _isolationClasses.TryGetValue(plugin, out var cssClass) ? cssClass : string.Empty;

        public void Initialize()
        {
            _plugins.Clear();
            _isolationClasses.Clear();
            var cssBuilder = new System.Text.StringBuilder();

            foreach (var plugin in _pluginLoader.DiscoverPlugins())
            {
                _plugins.Add(plugin);

                var assembly = plugin.GetType().Assembly;
                string cssClassName = "sp-" + Guid.NewGuid().ToString("N").Substring(0, 8);
                _isolationClasses[plugin] = cssClassName;

                foreach (var resourceName in assembly.GetManifestResourceNames().Where(r => r.EndsWith(".css", StringComparison.OrdinalIgnoreCase)))
                {
                    using var stream = assembly.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = new StreamReader(stream);
                        cssBuilder.AppendLine($"/* Loaded from {assembly.GetName().Name} -> {resourceName} */");
                        cssBuilder.AppendLine($".{cssClassName} {{");
                        cssBuilder.AppendLine(reader.ReadToEnd());
                        cssBuilder.AppendLine("}");
                    }
                }
            }

            AggregatedCss = cssBuilder.ToString();
        }

    }
}
