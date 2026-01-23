using System.Collections.Generic;
using Enterspeed.Query.Sdk.Api.Models.MultiQuery;

namespace Enterspeed.Query.Sdk.Api.Models.Query
{
    /// <summary>
    /// Internal model representing a successful query response from the multi-query API.
    /// Used for JSON deserialization. Users should interact with ISuccess&lt;T&gt; instead via MultiQueryApiResponse.Get&lt;T&gt;().
    /// </summary>
    public class MultiQueryResponseSuccess : MultiQueryResponse
    {
        public int TotalResults { get; set; }
        public List<Dictionary<string, object>> Results { get; set; } = new List<Dictionary<string, object>>();
        public List<FacetResult> Facets { get; set; } = new List<FacetResult>();
        public override QueryStatus Status => QueryStatus.Success;
    }
}
