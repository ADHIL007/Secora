

using System.Collections.ObjectModel;


namespace Secora.Abstractions
{
    public interface IPluginManager
    {
        IReadOnlyCollection<ISecoraPlugin> Plugins { get; }
        string AggregatedCss { get; }
        string GetCssIsolationClass(ISecoraPlugin plugin);
    }
}
