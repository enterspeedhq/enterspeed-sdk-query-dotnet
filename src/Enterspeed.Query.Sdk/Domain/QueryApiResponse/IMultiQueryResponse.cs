namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
{
     public abstract class MultiQueryResponse
     {
         public string Index { get; set; }
         public string Name { get; set; }
         public abstract QueryStatus Status { get; }
     }
}
