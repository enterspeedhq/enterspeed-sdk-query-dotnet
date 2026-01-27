namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
{
    /// <summary>
    /// Internal model representing a failed query response from the multi-query API.
    /// Used for JSON deserialization. Users should interact with IFailure instead via MultiQueryApiResponse.Get&lt;T&gt;().
    /// </summary>
    public class MultiQueryResponseError : MultiQueryResponse
    {
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public override QueryStatus Status => QueryStatus.Error;
    }
}
