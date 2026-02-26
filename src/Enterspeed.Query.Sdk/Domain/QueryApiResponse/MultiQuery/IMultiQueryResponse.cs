using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse.MultiQuery
{
    [JsonConverter(typeof(MultiQueryResponseConverter))]
     public abstract class MultiQueryResponse
     {
         public string Index { get; set; }
         public string Name { get; set; }
         public abstract QueryStatus Status { get; }
     }
}
