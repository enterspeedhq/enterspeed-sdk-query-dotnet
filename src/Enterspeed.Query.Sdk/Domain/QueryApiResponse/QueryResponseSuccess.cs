using System.Collections.Generic;
using Enterspeed.Query.Sdk.Api.Models;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
{
    public class QueryResponseSuccess : QueryResponse
    {
        public int TotalResults { get; set; }
        public List<Dictionary<string, object>> Results { get; set; } = new List<Dictionary<string, object>>();
        public List<FacetResult> Facets { get; set; } = new List<FacetResult>();
        public override QueryStatus Status => QueryStatus.Success;
    }

    public class QueryResponseSuccess<T> : QueryResponse<T>
    {
        public int TotalResults { get; set; }
        public List<T> Results { get; set; } = new List<T>();
        public List<FacetResult> Facets { get; set; } = new List<FacetResult>();
        public override QueryStatus Status => QueryStatus.Success;
    }
}
