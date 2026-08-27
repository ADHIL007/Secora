namespace Secora.Abstractions;

/// <summary>
/// Apply to controllers, actions, or minimal API endpoints to explicitly
/// exclude them from Secora's endpoint scanning and security analysis.
/// 
/// Usage on a controller:
/// <code>
/// [SecoraIgnore]
/// [ApiController]
/// public class InternalDebugController : ControllerBase { }
/// </code>
/// 
/// Usage on a specific action:
/// <code>
/// [SecoraIgnore]
/// [HttpGet("debug/info")]
/// public IActionResult GetDebugInfo() => Ok();
/// </code>
/// 
/// Usage on a minimal API:
/// <code>
/// app.MapGet("/internal", () => "hidden")
///    .WithMetadata(new SecoraIgnoreAttribute());
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class SecoraIgnoreAttribute : Attribute
{
    /// <summary>
    /// Optional reason for ignoring this endpoint (for audit trail).
    /// </summary>
    public string? Reason { get; set; }
}
