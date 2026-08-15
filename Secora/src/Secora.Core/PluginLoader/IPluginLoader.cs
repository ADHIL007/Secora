

using Secora.Abstractions;

namespace Secora.Core.PluginLoader
{
    public interface IPluginLoader
    {
        IEnumerable<ISecoraPlugin> DiscoverPlugins();


    }
}
