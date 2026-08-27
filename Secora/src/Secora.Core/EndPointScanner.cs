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
    /// <summary>
    /// Pre-computed summary statistics for the dashboard.
    /// Avoids the need to enumerate all endpoints just to get counts.
    /// </summary>
    public sealed class EndpointScanSummary
    {
        public int TotalApplicationEndpoints { get; init; }
        public int AuthenticatedEndpoints { get; init; }
        public int OpenEndpoints { get; init; }
        public int TotalInfrastructureEndpoints { get; init; }
        public int IgnoredEndpoints { get; init; }
        public double AuthPercentage { get; init; }
        public double OpenPercentage { get; init; }
    }

    public class EndPointScanner
    {
        private readonly IEnumerable<EndpointDataSource> _endpointDataSources;
        private readonly EndpointClassifier _classifier;

        // Lazy cache: endpoints are registered at startup and don't change.
        // Scan once, serve forever. Thread-safe for concurrent Blazor circuits.
        private readonly Lazy<IReadOnlyList<SecoraEndpoint>> _cachedEndpoints;
        private readonly Lazy<EndpointScanSummary> _cachedSummary;

        public EndPointScanner(
            IEnumerable<EndpointDataSource> endpointDataSources,
            EndpointClassifier classifier)
        {
            _endpointDataSources = endpointDataSources;
            _classifier = classifier;

            _cachedEndpoints = new Lazy<IReadOnlyList<SecoraEndpoint>>(
                ScanEndpoints,
                LazyThreadSafetyMode.ExecutionAndPublication);

            _cachedSummary = new Lazy<EndpointScanSummary>(
                () => ComputeSummary(_cachedEndpoints.Value),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Returns the cached, read-only list of all discovered endpoints.
        /// Safe to call from multiple Blazor circuits — scans only once.
        /// </summary>
        public IReadOnlyList<SecoraEndpoint> GetEndpoints() => _cachedEndpoints.Value;

        /// <summary>
        /// Returns pre-computed summary statistics.
        /// Dashboard should use this instead of enumerating GetEndpoints().
        /// </summary>
        public EndpointScanSummary GetSummary() => _cachedSummary.Value;

        public string GetEndpointsJson()
        {
            return JsonSerializer.Serialize(_cachedEndpoints.Value, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }

        /// <summary>
        /// Performs the actual scan. Called exactly once via Lazy&lt;T&gt;.
        /// 
        /// Memory strategy:
        ///   • Pre-sizes the list based on estimated count to avoid list resizing.
        ///   • Returns IReadOnlyList to prevent callers from adding/removing.
        ///   • Each SecoraEndpoint is allocated once and shared across all consumers.
        /// </summary>
        private IReadOnlyList<SecoraEndpoint> ScanEndpoints()
        {
            // Pre-count to avoid list resizing for large apps
            int estimatedCount = 0;
            foreach (var dataSource in _endpointDataSources)
            {
                estimatedCount += dataSource.Endpoints.Count;
            }

            var endpoints = new List<SecoraEndpoint>(estimatedCount);

            foreach (var dataSource in _endpointDataSources)
            {
                foreach (var endpoint in dataSource.Endpoints)
                {
                    if (endpoint is RouteEndpoint routeEndpoint)
                    {
                        var secoraEndpoint = MapEndpoint(routeEndpoint);
                        if (secoraEndpoint != null)
                        {
                            endpoints.Add(secoraEndpoint);
                        }
                    }
                }
            }

            // Trim excess capacity if we over-estimated (non-RouteEndpoints skipped)
            endpoints.TrimExcess();
            
            return endpoints.AsReadOnly();
        }

        /// <summary>
        /// Computes summary stats in a single pass over the cached list.
        /// </summary>
        private static EndpointScanSummary ComputeSummary(IReadOnlyList<SecoraEndpoint> endpoints)
        {
            int appTotal = 0, appAuth = 0, infraTotal = 0, ignoredTotal = 0;

            for (int i = 0; i < endpoints.Count; i++)
            {
                var ep = endpoints[i];

                if (EndpointClassifier.IsApplicationCode(ep.Category))
                {
                    appTotal++;
                    if (ep.RequiresAuthorization)
                        appAuth++;
                }
                else if (ep.Category == EndpointCategory.HiddenApi)
                {
                    ignoredTotal++;
                }
                else
                {
                    infraTotal++;
                }
            }

            int appOpen = appTotal - appAuth;

            return new EndpointScanSummary
            {
                TotalApplicationEndpoints = appTotal,
                AuthenticatedEndpoints = appAuth,
                OpenEndpoints = appOpen,
                TotalInfrastructureEndpoints = infraTotal,
                IgnoredEndpoints = ignoredTotal,
                AuthPercentage = appTotal > 0 ? Math.Round((double)appAuth / appTotal * 100, 1) : 0,
                OpenPercentage = appTotal > 0 ? Math.Round((double)appOpen / appTotal * 100, 1) : 0
            };
        }

        /// <summary>
        /// Maps a single RouteEndpoint to a SecoraEndpoint.
        /// Allocates only what's needed — empty collections use Array.Empty via list init.
        /// </summary>
        private SecoraEndpoint MapEndpoint(RouteEndpoint routeEndpoint)
        {
            var httpMethodMeta = routeEndpoint.Metadata.GetMetadata<IHttpMethodMetadata>();
            var methods = httpMethodMeta?.HttpMethods;

            var authData = routeEndpoint.Metadata.OfType<IAuthorizeData>().FirstOrDefault();

            var responseMeta = routeEndpoint.Metadata.OfType<IProducesResponseTypeMetadata>();
            List<SecoraResponseType>? responseTypes = null;

            foreach (var m in responseMeta)
            {
                responseTypes ??= new List<SecoraResponseType>();
                responseTypes.Add(new SecoraResponseType
                {
                    StatusCode = m.StatusCode,
                    TypeName = m.Type?.Name,
                    ContentTypes = m.ContentTypes?.ToList() ?? new List<string>()
                });
            }

            var rawPath = routeEndpoint.RoutePattern.RawText ?? string.Empty;

            return new SecoraEndpoint
            {
                Path = rawPath,
                Category = _classifier.Categorize(routeEndpoint),
                HttpMethods = methods?.ToList() ?? new List<string> { "GET" },
                DisplayName = routeEndpoint.DisplayName,
                RequiresAuthorization = authData != null,
                AuthPolicy = authData?.Policy,
                ResponseTypes = responseTypes ?? new List<SecoraResponseType>(),
                HandlerTypeName = routeEndpoint.RequestDelegate?.Target?.GetType().Name
            };
        }
    }
}
