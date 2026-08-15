
using System.Reflection;
using Secora.Abstractions;

namespace Secora.Core.PluginManager
{
    public class PluginDescriptor
    {
        public ISecoraPlugin Plugin { get; init; }

        public Assembly Assembly { get; init; }

        public string AssemblyName { get; init; }
    }
}
