using System.Reflection;
using System.Runtime.Loader;
using System.IO;
using Secora.Abstractions;

namespace Secora.Core.PluginLoader
{
    public class PluginLoader : IPluginLoader
    {
        private readonly List<Assembly> _assemblies = new();

        public PluginLoader()
        {
            LoadAssemblies();
        }
        private void LoadAssemblies()
        {
            foreach (string dll in Directory.GetFiles(
    AppContext.BaseDirectory,
    "*.SecoraPlugin.dll"))
            {
                Assembly assembly =
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);

                _assemblies.Add(assembly);
            }
        }

        public IEnumerable<ISecoraPlugin> DiscoverPlugins()
        {
            foreach (Assembly assembly in _assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(ISecoraPlugin).IsAssignableFrom(type)
                        && type.IsClass
                        && !type.IsAbstract)
                    {
                        object? instance = Activator.CreateInstance(type);

                        if (instance is ISecoraPlugin plugin)
                        {
                            yield return plugin;
                        }
                    }
                }
            }
        }
    }
}
