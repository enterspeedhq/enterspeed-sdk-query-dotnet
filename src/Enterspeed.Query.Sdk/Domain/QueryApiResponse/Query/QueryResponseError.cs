namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query
{
    public class QueryResponseError : IQueryResponse
    {
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public QueryStatus Status => QueryStatus.Error;
    }

    /// <summary>
    /// Generic version of QueryResponseError for type-safe error handling.
    /// Inherits converter from QueryResponse&lt;T&gt;
    /// </summary>
    public class QueryResponseError<T> : QueryResponse<T>
    {
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public override QueryStatus Status => QueryStatus.Error;
    }
}
