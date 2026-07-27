using Enterspeed.Query.Sdk.Domain.Models.LogicalOperators;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.Models
{
    public class QueryObject
    {
        [JsonPropertyName("filters")]
        public AndOperator Filters { get; set; }

        [JsonPropertyName("aliases")]
        public List<string> Aliases { get; set; }

        [JsonPropertyName("sort")]
        public List<Sort> Sort { get; set; }

        [JsonPropertyName("pagination")]
        public Pagination Pagination { get; set; }

        [JsonPropertyName("facets")]
        public List<Facet> Facets { get; set; }

        /// <summary>
        /// Optional relevance search. Omitted from the serialized request when null,
        /// so requests that do not use search are unchanged.
        /// </summary>
        [JsonPropertyName("search")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public QuerySearch Search { get; set; }
    }
}
