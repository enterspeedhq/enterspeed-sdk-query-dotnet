namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query
{
    public class QueryResponseError : QueryResponse, IQueryResponse<object>
    {
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public override QueryStatus Status => QueryStatus.Error;
    }
}
