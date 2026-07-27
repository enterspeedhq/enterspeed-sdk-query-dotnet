using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.Models
{
    /// <summary>
    /// Relevance search applied to a single text field of a query.
    /// Enables sorting by the reserved "_score" field, which the Query API rejects when no search is present.
    /// </summary>
    public class QuerySearch
    {
        /// <summary>
        /// The text field to search. Only fields of type text or text[] are supported by the Query API.
        /// </summary>
        [JsonPropertyName("field")]
        public string Field { get; set; }

        /// <summary>
        /// The value to search for.
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; }

        /// <summary>
        /// When true, matches indexed tokens exactly (no fuzziness). Defaults to false.
        /// </summary>
        [JsonPropertyName("literal")]
        public bool Literal { get; set; }
    }
}
