namespace Enterspeed.Query.Sdk.Api.Models
{
    public class MultiQueryResponse : QueryResponse
    {
        public string Index { get; set; }
        public string Name { get; set; }
    }

    public class MultiQueryResponse<T> : QueryResponse<T>
    {
        public string Index { get; set; }
        public string Name { get; set; }
    }
}