using System.Collections.Generic;
using Enterspeed.Query.Sdk.Api.Models.MultiQuery;

namespace Enterspeed.Query.Sdk.Api.Models.Query
{
    public class MultiQueryResponseSuccess : MultiQueryResponse
    {
        public int TotalResults { get; set; }
        public List<Dictionary<string, object>> Results { get; set; } = new List<Dictionary<string, object>>();
        public List<FacetResult> Facets { get; set; } = new List<FacetResult>();
        public override QueryStatus Status => QueryStatus.Success;
    }

    public class MultiQueryResponseSuccess<T> : MultiQueryResponse<T>
    {
        public int TotalResults { get; set; }
        public List<T> Results { get; set; } = new List<T>();
        public List<FacetResult> Facets { get; set; } = new List<FacetResult>();
        public override QueryStatus Status => QueryStatus.Success;
    }

}
