namespace Secora.Abstractions;

/// <summary>
/// Categorizes an endpoint by its true runtime nature — determined primarily
/// by inspecting endpoint metadata (not URL paths).
/// </summary>
public enum EndpointCategory
{
    /// <summary>Uncategorizable or empty path</summary>
    Unknown,

    // ── Application Code (Primary Scan Targets) ────────────
    /// <summary>Developer's API endpoint — the primary scan target</summary>
    ApplicationApi,

    /// <summary>MVC/Web API controller action</summary>
    ApplicationController,

    /// <summary>Minimal API endpoint (MapGet, MapPost, etc.)</summary>
    ApplicationMinimalApi,

    /// <summary>Razor Page endpoint</summary>
    ApplicationRazorPage,

    // ── Explicitly Ignored ─────────────────────────────────
    /// <summary>Endpoint tagged with [SecoraIgnore] or [ApiExplorerSettings(IgnoreApi = true)]</summary>
    HiddenApi,

    // ── Blazor ─────────────────────────────────────────────
    /// <summary>Blazor Server SignalR hub (ComponentHub)</summary>
    InfrastructureBlazorSignalR,

    /// <summary>Blazor WebAssembly framework files (/_framework)</summary>
    InfrastructureBlazorWasm,

    /// <summary>Blazor component/page endpoint</summary>
    InfrastructureBlazorComponent,

    // ── Static Assets ──────────────────────────────────────
    /// <summary>RCL or wwwroot static content</summary>
    InfrastructureStatic,

    // ── API Documentation ──────────────────────────────────
    /// <summary>Swagger / Swashbuckle / Scalar / ReDoc endpoints</summary>
    InfrastructureSwagger,

    /// <summary>Scalar API documentation endpoints</summary>
    InfrastructureScalar,

    // ── Self-Reference ─────────────────────────────────────
    /// <summary>Secora's own routes — excluded from scans to avoid loops</summary>
    InfrastructureSecora,

    // ── Health & Probes ────────────────────────────────────
    /// <summary>Health checks, readiness/liveness probes</summary>
    InfrastructureHealth,

    // ── Identity & Auth ────────────────────────────────────
    /// <summary>ASP.NET Core Identity, Duende IdentityServer, OIDC</summary>
    InfrastructureIdentity,

    // ── Real-Time ──────────────────────────────────────────
    /// <summary>SignalR hubs (non-Blazor)</summary>
    InfrastructureSignalR,

    // ── gRPC ────────────────────────────────────────────────
    /// <summary>gRPC service endpoints</summary>
    InfrastructureGrpc,

    // ── Diagnostics ────────────────────────────────────────
    /// <summary>Debug, diagnostics, profiling endpoints</summary>
    InfrastructureDiagnostics,

    // ── Fallback / Catch-All ───────────────────────────────
    /// <summary>Catch-all routes like {*path} for SPA fallback</summary>
    InfrastructureFallback,

    // ── Catch-All ──────────────────────────────────────────
    /// <summary>Infrastructure that doesn't fit other categories</summary>
    InfrastructureOther
}
