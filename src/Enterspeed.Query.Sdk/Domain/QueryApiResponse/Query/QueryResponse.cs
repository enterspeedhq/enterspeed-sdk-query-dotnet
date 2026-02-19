namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query
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
