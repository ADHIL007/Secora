using Secora.Abstractions;

namespace Sample.SecoraPlugin;

public class SamplePlugin : ISecoraPlugin
{
    private string _name = "Sample";

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

    string ISecoraPlugin.Description => "Sample plugin for testing";

    IPluginUiPage ISecoraPlugin.Page => throw new NotImplementedException();
}