using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
{
    [JsonConverter(typeof(QueryResponseConverter))]
    public abstract class QueryResponse : IQueryResponse
    {
        public abstract QueryStatus Status { get; }
    }

    [JsonConverter(typeof(QueryResponseConverter))]
    public abstract class QueryResponse<T> : IQueryResponse<T>
    {
        public abstract QueryStatus Status { get; }
    }
}
