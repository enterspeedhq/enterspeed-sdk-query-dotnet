using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query
{
    [JsonConverter(typeof(QueryResponseConverter))]

    public abstract class QueryResponse
    {
        public string Name { get; set; }
        public string Index { get; set; }
        public abstract QueryStatus Status { get; }
    }
}
