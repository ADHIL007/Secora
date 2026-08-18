using Secora.Abstractions;

namespace Sample.SecoraPlugin;

/// <summary>
/// Provides the Blazor component type for the Sample plugin's UI page.
/// </summary>
public class SamplePluginUiPage : IPluginUiPage
{
    public string Title => "Sample Plugin";
    public Type ComponentType => typeof(SamplePage);
}
