using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Api.Models
{

    // [JsonDerivedType(typeof(QueryResponse))]
    // [JsonDerivedType(typeof(QueryResponseError))]
    // [JsonDerivedType(typeof(MultiQueryResponse))]
    [JsonConverter(typeof(QueryResponseConverter))]

    public interface IQueryResponse
    {
       QueryStatus Status { get; }
    }


    // [JsonDerivedType(typeof(QueryResponse))]
    // [JsonDerivedType(typeof(QueryResponseError))]
    // [JsonDerivedType(typeof(MultiQueryResponse))]
    [JsonConverter(typeof(QueryResponseConverter))]

    public interface IQueryResponse<T>
    {
        QueryStatus Status { get; }
    }
}
