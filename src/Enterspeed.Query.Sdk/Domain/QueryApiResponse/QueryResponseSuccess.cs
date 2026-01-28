using System.Collections.Generic;
using Enterspeed.Query.Sdk.Api.Models;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
{
    // TODO: Rewrite this comment Use to convert into a success response from the Query API, however not for anything else
    public class QueryResponseSuccess : QueryResponse
    {
        public int TotalResults { get; set; }
        public List<Dictionary<string, object>> Results { get; set; } = new List<Dictionary<string, object>>();
        public List<FacetResult> Facets { get; set; } = new List<FacetResult>();
        public override QueryStatus Status => QueryStatus.Success;
    }

    // TODO: Rewrite this comment Used to get each typed result from the Query API
    public class QueryResponseSuccess<T> : QueryResponse<T>
    {
        public int TotalResults { get; set; }
        public List<T> Results { get; set; } = new List<T>();
        public List<FacetResult> Facets { get; set; } = new List<FacetResult>();
        public override QueryStatus Status => QueryStatus.Success;
    }
}
