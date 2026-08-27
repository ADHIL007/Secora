namespace Secora.Abstractions;

/// <summary>
/// Configuration options for Secora. 
/// Configured via the options pattern in Program.cs:
/// <code>
/// builder.Services.AddSecora(options =>
/// {
///     options.RoutePrefix = "/security-dashboard";
///     options.AdditionalInfrastructurePrefixes.Add("/custom-health");
///     options.HealthCheckPrefixes.Add("/my-health");
/// });
/// </code>
/// </summary>
public class SecoraOptions
{
    /// <summary>
    /// The route prefix for Secora's own UI.
    /// Default: "/secora"
    /// If changed, EndpointClassifier will use this to identify Secora's own routes.
    /// </summary>
    public string RoutePrefix { get; set; } = "/secora";

    /// <summary>
    /// Known health check / probe prefixes.
    /// These are auto-detected as Infrastructure:Health.
    /// Add your custom health endpoints here.
    /// </summary>
    public List<string> HealthCheckPrefixes { get; set; } = new()
    {
        "/health",
        "/healthz",
        "/healthcheck",
        "/ready",
        "/readyz",
        "/readiness",
        "/live",
        "/livez",
        "/liveness",
        "/ping",
        "/status",
        "/startup"
    };

    /// <summary>
    /// Known identity / auth endpoint prefixes.
    /// Add custom OIDC or Identity endpoints here.
    /// </summary>
    public List<string> IdentityPrefixes { get; set; } = new()
    {
        "/connect",
        "/identity",
        "/.well-known",
        "/account",
        "/manage",
        "/_configuration"
    };

    /// <summary>
    /// Known diagnostics / dev-tool prefixes.
    /// </summary>
    public List<string> DiagnosticsPrefixes { get; set; } = new()
    {
        "/_vs",
        "/_debug",
        "/metrics",
        "/trace",
        "/profiler",
        "/elmah",
        "/hangfire",
        "/mini-profiler"
    };

    /// <summary>
    /// Known API documentation prefixes.
    /// </summary>
    public List<string> ApiDocsPrefixes { get; set; } = new()
    {
        "/swagger",
        "/scalar",
        "/redoc",
        "/api-docs"
    };

    /// <summary>
    /// Known SignalR hub prefixes (non-Blazor).
    /// </summary>
    public List<string> SignalRPrefixes { get; set; } = new()
    {
        "/hubs"
    };

    /// <summary>
    /// Known gRPC prefixes.
    /// </summary>
    public List<string> GrpcPrefixes { get; set; } = new()
    {
        "/grpc"
    };

    /// <summary>
    /// Any additional infrastructure prefixes that don't fit the above categories.
    /// These will be classified as InfrastructureOther.
    /// </summary>
    public List<string> AdditionalInfrastructurePrefixes { get; set; } = new();

    /// <summary>
    /// File extensions considered as static files when served via mapped endpoints.
    /// </summary>
    public List<string> StaticFileExtensions { get; set; } = new()
    {
        ".js", ".css", ".map", ".json",
        ".png", ".jpg", ".jpeg", ".gif", ".svg", ".ico", ".webp",
        ".woff", ".woff2", ".ttf", ".eot",
        ".html", ".htm",
        ".wasm", ".dll", ".pdb",
        ".xml", ".txt"
    };
}
