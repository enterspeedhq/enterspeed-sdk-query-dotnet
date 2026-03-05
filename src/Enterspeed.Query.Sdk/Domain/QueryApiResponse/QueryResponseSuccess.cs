using System.Collections.Generic;
using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query
{
    /// <summary>
    /// Represents a successful response from the Enterspeed Query API containing typed results.
    /// This class encapsulates the query results, total count, and facet information returned
    /// when a query operation completes successfully.
    /// </summary>
    /// <typeparam name="T">The type of objects returned in the Results collection</typeparam>

    public class QueryResponseSuccess<T> : QueryResponse, IQueryResponse<T>
    {
        public int TotalResults { get; set; }
        public List<T> Results { get; set; } = new List<T>();
        public List<FacetResult> Facets { get; set; } = new List<FacetResult>();
        public override QueryStatus Status => QueryStatus.Success;
    }
}
