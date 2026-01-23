using Enterspeed.Query.Sdk.Api.Models.MultiQuery;

namespace Enterspeed.Query.Sdk.Api.Models.Query
{
    public class MultiQueryResponseError : MultiQueryResponse
    {
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public override QueryStatus Status => QueryStatus.Error;
    }

    public class MultiQueryResponseError<T> : MultiQueryResponse<T>
    {
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public override QueryStatus Status => QueryStatus.Error;
    }
}
