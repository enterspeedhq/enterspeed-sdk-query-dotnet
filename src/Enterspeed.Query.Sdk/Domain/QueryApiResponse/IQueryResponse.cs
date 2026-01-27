using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Api.Models.Response;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
{
    [JsonConverter(typeof(QueryResponseConverter))]

    public interface IQueryResponse
    {
       QueryStatus Status { get; }
    }

    [JsonConverter(typeof(QueryResponseConverter))]

    public interface IQueryResponse<T>
    {
        QueryStatus Status { get; }
    }
}
