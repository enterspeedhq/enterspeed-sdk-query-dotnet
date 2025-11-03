using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Api.Models
{
    public class QueryResponse
    {
        public int TotalResults { get; set; }
        public List<Dictionary<string, object>> Results { get; set; } = new List<Dictionary<string, object>>();
        public List<Facet> Facets { get; set; } = new List<Facet>();
    }

    public class QueryResponse<T>
    {
        public int TotalResults { get; set; }
        public List<T> Results { get; set; } = new List<T>();
        public List<Facet> Facets { get; set; } = new List<Facet>();
    }
}