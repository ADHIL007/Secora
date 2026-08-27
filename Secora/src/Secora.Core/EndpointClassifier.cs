using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Secora.Abstractions;

namespace Secora.Core;

/// <summary>
/// Classifies endpoints using a metadata-first strategy.
/// 
/// Classification priority:
///   1. Explicit developer opt-out: [SecoraIgnore] or [ApiExplorerSettings(IgnoreApi = true)]
///   2. Metadata inspection: Inspect endpoint.Metadata for known framework types
///      (HubMetadata, ControllerActionDescriptor, PageActionDescriptor, gRPC markers, etc.)
///   3. Path-based fallback: Only used as a tie-breaker when metadata is sparse
///      (e.g., static assets served via UseStaticFiles, health checks without rich metadata)
///
/// Why not path-based?
///   • "/api/users/_framework/details" is a legitimate API — path matching would hide it
///   • app.MapHub&lt;T&gt;("/my-custom-path") wouldn't be detected by path alone
///   • An attacker mapping sensitive endpoints to "/_content/..." would be invisible
/// </summary>
public class EndpointClassifier
{
    private readonly SecoraOptions _options;

    // gRPC metadata type name — resolved via reflection to avoid hard NuGet dependency
    private static readonly Type? GrpcMethodMetadataType = Type.GetType(
        "Grpc.AspNetCore.Server.Model.GrpcMethodMetadata, Grpc.AspNetCore.Server",
        throwOnError: false);

    // Blazor component metadata — resolved via reflection for forward compatibility
    private static readonly Type? ComponentTypeMetadataType = Type.GetType(
        "Microsoft.AspNetCore.Components.Endpoints.ComponentTypeMetadata, Microsoft.AspNetCore.Components.Endpoints",
        throwOnError: false);

    // Health check metadata — resolved via reflection since it's in a separate package
    private static readonly Type? HealthCheckOptionsType = Type.GetType(
        "Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions, Microsoft.AspNetCore.Diagnostics.HealthChecks",
        throwOnError: false);

    public EndpointClassifier(IOptions<SecoraOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Classify an endpoint by inspecting its metadata collection.
    /// The <paramref name="endpoint"/> provides the full metadata;
    /// the path is only used as a secondary signal.
    /// </summary>
    public EndpointCategory Categorize(Endpoint endpoint)
    {
        if (endpoint is not RouteEndpoint routeEndpoint)
            return EndpointCategory.Unknown;

        var metadata = endpoint.Metadata;
        var path = routeEndpoint.RoutePattern.RawText ?? string.Empty;

        // ════════════════════════════════════════════════════════
        // LAYER 1: Explicit developer opt-out (highest priority)
        // ════════════════════════════════════════════════════════

        // [SecoraIgnore] attribute — developer explicitly wants this hidden
        if (metadata.GetMetadata<SecoraIgnoreAttribute>() != null)
            return EndpointCategory.HiddenApi;

        // [ApiExplorerSettings(IgnoreApi = true)] — respect API explorer settings
        var apiExplorerSettings = metadata.GetMetadata<ApiExplorerSettingsAttribute>();
        if (apiExplorerSettings is { IgnoreApi: true })
            return EndpointCategory.HiddenApi;

        // ════════════════════════════════════════════════════════
        // LAYER 2: Known Infrastructure via Metadata
        // ════════════════════════════════════════════════════════

        // ── SignalR Hubs ──
        var hubMetadata = metadata.GetMetadata<HubMetadata>();
        if (hubMetadata != null)
        {
            var hubTypeName = hubMetadata.HubType?.FullName ?? "";
            if (hubTypeName.Contains("Blazor", StringComparison.OrdinalIgnoreCase) ||
                hubTypeName.Contains("ComponentHub", StringComparison.OrdinalIgnoreCase))
            {
                return EndpointCategory.InfrastructureBlazorSignalR;
            }
            return EndpointCategory.InfrastructureSignalR;
        }

        // ── gRPC endpoints ──
        if (GrpcMethodMetadataType != null && HasMetadataOfType(metadata, GrpcMethodMetadataType))
            return EndpointCategory.InfrastructureGrpc;

        // ── Blazor component pages ──
        if (ComponentTypeMetadataType != null && HasMetadataOfType(metadata, ComponentTypeMetadataType))
            return EndpointCategory.InfrastructureBlazorComponent;

        // ── Health Checks ──
        if (IsHealthCheckEndpoint(endpoint))
            return EndpointCategory.InfrastructureHealth;

        // ════════════════════════════════════════════════════════
        // LAYER 3: REST API Discrimination
        // ════════════════════════════════════════════════════════
        
        var hasHttpMethod = metadata.GetMetadata<IHttpMethodMetadata>() != null;
        var controllerDescriptor = metadata.GetMetadata<ControllerActionDescriptor>();
        var pageDescriptor = metadata.GetMetadata<PageActionDescriptor>();

        // A standard user API MUST either have HTTP methods defined or be a Controller/Razor Page.
        // If it lacks all of these, it is some form of internal infrastructure (like Blazor initialization, 
        // static files, or WebSockets).
        bool isRestApi = hasHttpMethod || controllerDescriptor != null || pageDescriptor != null;

        if (!isRestApi)
        {
            // It's not a REST API. Check if it matches known infra paths (e.g. static files).
            var nonRestPathCat = CategorizeByPath(path);
            if (nonRestPathCat != EndpointCategory.Unknown)
                return nonRestPathCat;

            // Otherwise, it's generic infrastructure, but definitely NOT a User API.
            return EndpointCategory.InfrastructureOther;
        }

        // ════════════════════════════════════════════════════════
        // LAYER 4: Known Infrastructure REST APIs
        // ════════════════════════════════════════════════════════

        // Check if this controller is part of Secora itself
        if (controllerDescriptor != null)
        {
            var controllerNamespace = controllerDescriptor.ControllerTypeInfo.Namespace ?? "";
            if (controllerNamespace.StartsWith("Secora", StringComparison.OrdinalIgnoreCase))
                return EndpointCategory.InfrastructureSecora;
        }

        // Check path fallbacks for known infra REST endpoints (Swagger, Health, Diagnostics, Identity)
        var pathCategory = CategorizeByPath(path);
        if (pathCategory != EndpointCategory.Unknown)
            return pathCategory;

        // ════════════════════════════════════════════════════════
        // LAYER 5: Default — Developer Application Code
        // ════════════════════════════════════════════════════════

        if (controllerDescriptor != null)
            return EndpointCategory.ApplicationController;

        if (pageDescriptor != null)
            return EndpointCategory.ApplicationRazorPage;

        return EndpointCategory.ApplicationMinimalApi;
    }

    /// <summary>
    /// Detects health check endpoints via display name and metadata.
    /// MapHealthChecks() sets DisplayName = "Health checks".
    /// </summary>
    private bool IsHealthCheckEndpoint(Endpoint endpoint)
    {
        // Primary: display name set by MapHealthChecks()
        if (endpoint.DisplayName != null &&
            endpoint.DisplayName.Contains("Health check", StringComparison.OrdinalIgnoreCase))
            return true;

        // Secondary: HealthCheckOptions in metadata (if the type is available)
        if (HealthCheckOptionsType != null && HasMetadataOfType(endpoint.Metadata, HealthCheckOptionsType))
            return true;

        return false;
    }

    /// <summary>
    /// Path-based classification — used ONLY as a fallback when metadata is sparse.
    /// Returns Unknown if the path doesn't match any known infrastructure pattern,
    /// allowing the caller to fall through to application-code defaults.
    /// </summary>
    private EndpointCategory CategorizeByPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return EndpointCategory.Unknown;

        var normalized = (path.StartsWith("/") ? path : "/" + path).ToLowerInvariant();

        // Catch-all / fallback routes
        if (normalized.Contains("{*") || normalized.Contains("{**"))
            return EndpointCategory.InfrastructureFallback;

        // Blazor HTTP helper endpoints (/_blazor/disconnect, /_blazor/initializers, etc.)
        // These are plain HTTP endpoints — NOT SignalR hubs — so HubMetadata is absent.
        // The actual SignalR hub is caught by Layer 2's HubMetadata check.
        if (IsSegmentMatch(normalized, "/_blazor"))
            return EndpointCategory.InfrastructureBlazorSignalR;

        // Blazor WASM framework files (no rich metadata, served via static file provider)
        if (IsSegmentMatch(normalized, "/_framework"))
            return EndpointCategory.InfrastructureBlazorWasm;

        // RCL static content
        if (IsSegmentMatch(normalized, "/_content"))
            return EndpointCategory.InfrastructureStatic;

        // Secora's own UI (configurable via SecoraOptions.RoutePrefix)
        if (IsSegmentMatch(normalized, _options.RoutePrefix.ToLowerInvariant()))
            return EndpointCategory.InfrastructureSecora;

        // Health check paths (configurable via SecoraOptions.HealthCheckPrefixes)
        foreach (var prefix in _options.HealthCheckPrefixes)
        {
            if (IsSegmentMatch(normalized, prefix.ToLowerInvariant()))
                return EndpointCategory.InfrastructureHealth;
        }

        // API docs paths (configurable)
        foreach (var prefix in _options.ApiDocsPrefixes)
        {
            if (IsSegmentMatch(normalized, prefix.ToLowerInvariant()))
                return EndpointCategory.InfrastructureSwagger;
        }

        // Identity paths (configurable)
        foreach (var prefix in _options.IdentityPrefixes)
        {
            if (IsSegmentMatch(normalized, prefix.ToLowerInvariant()))
                return EndpointCategory.InfrastructureIdentity;
        }

        // Diagnostics paths (configurable)
        foreach (var prefix in _options.DiagnosticsPrefixes)
        {
            if (IsSegmentMatch(normalized, prefix.ToLowerInvariant()))
                return EndpointCategory.InfrastructureDiagnostics;
        }

        // User-defined additional infrastructure
        foreach (var prefix in _options.AdditionalInfrastructurePrefixes)
        {
            if (IsSegmentMatch(normalized, prefix.ToLowerInvariant()))
                return EndpointCategory.InfrastructureOther;
        }

        // Static file extensions (endpoints serving .js, .css, .wasm, etc.)
        if (HasStaticFileExtension(normalized))
            return EndpointCategory.InfrastructureStatic;

        return EndpointCategory.Unknown;
    }

    /// <summary>
    /// Segment-boundary matching. Prevents "/secora-admin" from matching "/secora".
    /// Matches: exact path, path + "/", path + "?"
    /// </summary>
    private static bool IsSegmentMatch(string normalizedPath, string prefix)
    {
        if (normalizedPath.Length == prefix.Length)
            return normalizedPath == prefix;

        if (normalizedPath.Length > prefix.Length && normalizedPath.StartsWith(prefix))
        {
            char next = normalizedPath[prefix.Length];
            return next == '/' || next == '?';
        }

        return false;
    }

    /// <summary>
    /// Checks for static file extensions. Strips query string first.
    /// </summary>
    private bool HasStaticFileExtension(string normalizedPath)
    {
        var pathOnly = normalizedPath.Contains('?')
            ? normalizedPath[..normalizedPath.IndexOf('?')]
            : normalizedPath;

        foreach (var ext in _options.StaticFileExtensions)
        {
            if (pathOnly.EndsWith(ext.ToLowerInvariant()))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Reflection-based metadata check for types not in the shared framework
    /// (e.g., Grpc.AspNetCore.Server). Avoids hard NuGet dependencies.
    /// </summary>
    private static bool HasMetadataOfType(EndpointMetadataCollection metadata, Type type)
    {
        foreach (var item in metadata)
        {
            if (type.IsInstanceOfType(item))
                return true;
        }
        return false;
    }

    // ═══════════════════════════════════════════════════════════
    // Static helper methods (used by UI components)
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Human-friendly display label for the category.
    /// </summary>
    public static string GetDisplayLabel(EndpointCategory category)
    {
        return category switch
        {
            EndpointCategory.ApplicationApi => "API",
            EndpointCategory.ApplicationController => "Controller",
            EndpointCategory.ApplicationMinimalApi => "Minimal API",
            EndpointCategory.ApplicationRazorPage => "Razor Page",
            EndpointCategory.HiddenApi => "Hidden API",
            EndpointCategory.InfrastructureBlazorSignalR => "Blazor SignalR",
            EndpointCategory.InfrastructureBlazorWasm => "Blazor WASM",
            EndpointCategory.InfrastructureBlazorComponent => "Blazor Component",
            EndpointCategory.InfrastructureStatic => "Static Asset",
            EndpointCategory.InfrastructureSwagger => "API Docs",
            EndpointCategory.InfrastructureScalar => "Scalar",
            EndpointCategory.InfrastructureSecora => "Secora",
            EndpointCategory.InfrastructureHealth => "Health Check",
            EndpointCategory.InfrastructureIdentity => "Identity/Auth",
            EndpointCategory.InfrastructureSignalR => "SignalR Hub",
            EndpointCategory.InfrastructureGrpc => "gRPC",
            EndpointCategory.InfrastructureDiagnostics => "Diagnostics",
            EndpointCategory.InfrastructureFallback => "Fallback",
            EndpointCategory.InfrastructureOther => "Other",
            EndpointCategory.Unknown => "Unknown",
            _ => category.ToString()
        };
    }

    /// <summary>
    /// Returns true if the category is any Infrastructure variant or Ignored.
    /// </summary>
    public static bool IsInfrastructure(EndpointCategory category)
    {
        return category != EndpointCategory.ApplicationApi
            && category != EndpointCategory.ApplicationController
            && category != EndpointCategory.ApplicationMinimalApi
            && category != EndpointCategory.ApplicationRazorPage
            && category != EndpointCategory.Unknown;
    }

    /// <summary>
    /// Returns true if the category represents application code (scan targets).
    /// </summary>
    public static bool IsApplicationCode(EndpointCategory category)
    {
        return category == EndpointCategory.ApplicationApi
            || category == EndpointCategory.ApplicationController
            || category == EndpointCategory.ApplicationMinimalApi
            || category == EndpointCategory.ApplicationRazorPage;
    }
}
