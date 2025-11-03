using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;

namespace Enterspeed.Query.Sdk.Api.Models
{
    public class QueryApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public QueryResponse Response { get; set; }
    }

    public class QueryApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public QueryResponse<T> Response { get; set; }
    }
    public class MultiQueryApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public List<MultiQueryResponse> Response { get; set; }
    }

    public class MultiQueryApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public List<MultiQueryResponse<T>> Response { get; set; }
    }
}