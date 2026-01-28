using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Api.Models.Response;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
{
    public abstract class QueryResponse : IQueryResponse
    {
        public abstract QueryStatus Status { get; }
    }

    public abstract class QueryResponse<T> : IQueryResponse<T>
    {
        public abstract QueryStatus Status { get; }
    }
}
