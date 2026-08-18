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
    public static class ApplicationBuilderExtensions
    {
        public static WebApplication UseSecora(this WebApplication app)
        {
            app.MapStaticAssets();
            app.UseAntiforgery();
            app.MapSecora();

            return app;
        }
    }
}
