using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Api.Models.Response;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
{
    [JsonConverter(typeof(QueryResponseConverter))]

    public class QueryResponseError : IQueryResponse
    {
        public string Message { get; set; }
        public string[] Errors { get; set; }
        public QueryStatus Status => QueryStatus.Error;
    }
}
