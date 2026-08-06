

using System.Collections.ObjectModel;

namespace Secora.Abstractions
{
    internal interface IPluginManager
    {
        public ReadOnlyCollection<ITestPlugin> plugins { get; }
    }
}
