using Secora.Abstractions;

namespace SecurityHeaders.SecoraPlugin;

public class SecurityHeadersPlugin : ISecoraPlugin
{
    private string _name = "Security Headers";

    string ISecoraPlugin.Name
    {
        get => _name;
        set => _name = value;
    }
    Version ISecoraPlugin.Version
    {
        get => new Version(1, 0, 0);
        set { }
    }

    string ISecoraPlugin.Author => "Secora";

    string ISecoraPlugin.Description => "Security Headers plugin for testing";

    IPluginUiPage ISecoraPlugin.Page => new SecurityHeadersPluginUiPage();
}