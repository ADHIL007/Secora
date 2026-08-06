namespace Secora.Abstractions
{
    public interface ITestPlugin
    {
        string Name { get; set; }

        Version Version { get; set; }
        string Author { get; }
        string Description { get; }
    }
}
