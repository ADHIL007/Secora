using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Secora.UI.Blazor;

namespace Secora.AspNetCore.Extensions
{
    public static class EndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapSecora(
         this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapRazorComponents<SecoraApp>();

            return endpoints;
        }
    }
}
