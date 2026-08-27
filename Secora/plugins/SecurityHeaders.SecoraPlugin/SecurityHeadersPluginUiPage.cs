using Secora.Abstractions;

namespace SecurityHeaders.SecoraPlugin;

/// <summary>
/// Provides the Blazor component type for the Security Headers plugin's UI page.
/// </summary>
public class SecurityHeadersPluginUiPage : IPluginUiPage
{
    public string Title => "Security Headers Plugin";
    public Type ComponentType => typeof(SecurityHeadersPage);
}
