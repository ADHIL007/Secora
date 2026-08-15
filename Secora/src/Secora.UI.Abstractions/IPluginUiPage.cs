namespace Secora.Abstractions
{
    public interface IPluginUiPage
    {
        string Title { get; }
        Type ComponentType { get; }
    }
}
