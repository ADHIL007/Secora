namespace Secora.Abstractions
{
    internal interface IPluginUiPage
    {
        string Title { get; }
        Type ComponentType { get; }
    }
}
