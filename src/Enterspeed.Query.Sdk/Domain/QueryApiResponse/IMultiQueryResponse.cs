using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Api.Models.Response;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
{
    [JsonConverter(typeof(MultiQueryResponseConverter))]
     public interface IMultiQueryResponse : IQueryResponse
     {
         string Name { get; }
         string Index { get; }
     }

     [JsonConverter(typeof(MultiQueryResponseConverter))]
     public abstract class MultiQueryResponse
     {
         public string Index { get; set; }
         public string Name { get; set; }
         public abstract QueryStatus Status { get; }
     }

     [JsonConverter(typeof(MultiQueryResponseConverter))]
     public interface IMultiQueryResponse<T> : IQueryResponse
     {
         string Name { get; }
         string Index { get; }
     }

     [JsonConverter(typeof(MultiQueryResponseConverter))]
     public abstract class MultiQueryResponse<T>
     {
         public string Index { get; set; }
         public string Name { get; set; }
         public abstract QueryStatus Status { get; }
     }
}
