namespace Secora.Abstractions;

/// <summary>
/// A simplified representation of an API endpoint for the Secora UI.
/// </summary>
public class SecoraEndpoint
{
    private static int _idCounter;
    private Dictionary<string, object?>? _extendedMetadata;

    /// <summary>
    /// Unique ID for this endpoint (useful for plugin mapping).
    /// Uses an incrementing counter instead of Guid to avoid 32-byte string allocation per endpoint.
    /// </summary>
    public string Id { get; set; } = Interlocked.Increment(ref _idCounter).ToString();

    /// <summary>
    /// The HTTP Path pattern (e.g., "/api/products/{id}")
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Classification of this endpoint (Application API vs Infrastructure)
    /// </summary>
    public EndpointCategory Category { get; set; } = EndpointCategory.ApplicationApi;

    /// <summary>
    /// HTTP Methods allowed (GET, POST, etc.)
    /// </summary>
    public List<string> HttpMethods { get; set; } = new();

    /// <summary>
    /// Display Name (often the method name or route name)
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Is this endpoint protected by [Authorize]?
    /// </summary>
    public bool RequiresAuthorization { get; set; }

    /// <summary>
    /// Specific Auth Policy name if defined
    /// </summary>
    public string? AuthPolicy { get; set; }

    /// <summary>
    /// Expected Response Types (Status Code -> Type Name)
    /// </summary>
    public List<SecoraResponseType> ResponseTypes { get; set; } = new();

    /// <summary>
    /// The underlying .NET Type of the handler method (if available via reflection)
    /// Useful for advanced plugins needing deep inspection.
    /// </summary>
    public string? HandlerTypeName { get; set; }

    /// <summary>
    /// Optional: Raw metadata dump for advanced plugin extensibility.
    /// Lazy-initialized — no allocation unless a plugin actually writes to it.
    /// </summary>
    public Dictionary<string, object?> ExtendedMetadata
    {
        get => _extendedMetadata ??= new Dictionary<string, object?>();
        set => _extendedMetadata = value;
    }
}

/// <summary>
/// Represents a specific response status code and type
/// </summary>
public class SecoraResponseType
{
    public int StatusCode { get; set; }
    public string? TypeName { get; set; }
    public List<string> ContentTypes { get; set; } = new();
}