using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Api.Models
{
    [JsonConverter(typeof(QueryResponseConverter))]

    public class QueryResponseError : IQueryResponse
    {
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public QueryStatus Status => QueryStatus.Error;
    }
}
