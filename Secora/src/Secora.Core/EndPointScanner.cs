using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Secora.Abstractions;

namespace Secora.Core
{
    public class EndPointScanner
    {
        private readonly IEnumerable<EndpointDataSource> _endpointDataSources;

        public EndPointScanner(IEnumerable<EndpointDataSource> endpointDataSources)
        {
            _endpointDataSources = endpointDataSources;
        }

        public string GetEndpointsJson()
        {
            var endpoints = GetEndpoints();
            return JsonSerializer.Serialize(endpoints, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }

        public IEnumerable<SecoraEndpoint> GetEndpoints()
        {
            var endpoints = new List<SecoraEndpoint>();

            foreach (var dataSource in _endpointDataSources)
            {
                foreach (var endpoint in dataSource.Endpoints)
                {
                    if (endpoint is RouteEndpoint routeEndpoint)
                    {
                        var secoraEndpoint = ToSecoraEndpoint(routeEndpoint);
                        if (secoraEndpoint != null)
                        {
                            endpoints.Add(secoraEndpoint);
                        }
                    }
                }
            }
            
            return endpoints;
        }

        public static SecoraEndpoint? ToSecoraEndpoint(Endpoint endpoint)
        {
            if (endpoint is not RouteEndpoint routeEndpoint)
                return null;

            var httpMethodMeta = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>();
            var methods = httpMethodMeta?.HttpMethods ?? new List<string> { "GET" };

            var authData = endpoint.Metadata.OfType<IAuthorizeData>().FirstOrDefault();
            bool isAuthorized = authData != null;

            var responseTypes = endpoint.Metadata
                .OfType<IProducesResponseTypeMetadata>()
                .Select(m => new SecoraResponseType
                {
                    StatusCode = m.StatusCode,
                    TypeName = m.Type?.Name,
                    ContentTypes = m.ContentTypes?.ToList() ?? new List<string>()
                })
                .ToList();

            return new SecoraEndpoint
            {
                Path = routeEndpoint.RoutePattern.RawText ?? string.Empty,
                HttpMethods = methods.ToList(),
                DisplayName = endpoint.DisplayName,
                RequiresAuthorization = isAuthorized,
                AuthPolicy = authData?.Policy,
                ResponseTypes = responseTypes,
                HandlerTypeName = endpoint.RequestDelegate?.Target?.GetType().Name
            };
        }
    }
}
