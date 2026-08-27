namespace Secora.Abstractions
{
    public interface ISecoraPlugin
    {
        string Name { get; set; }

        Version Version { get; set; }
        string Author { get; }
        string Description { get; }
        IPluginUiPage Page { get; }
    }
}
