using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Secora.UI.Blazor;

namespace Secora.AspNetCore.Extensions
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapSecora(
         this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapRazorComponents<App>()
                     .AddInteractiveServerRenderMode();

            return endpoints;
        }
    }
}
