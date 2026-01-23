using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using Enterspeed.Query.Sdk.Api.Models.MultiQuery;
using Enterspeed.Query.Sdk.Api.Models.Query;

namespace Enterspeed.Query.Sdk.Api.Models
{
    public class QueryApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public IQueryResponse Response { get; set; }
    }

    public class QueryApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public IQueryResponse<T> Response { get; set; }
    }
    public class MultiQueryApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public MultiQueryResponseList Response { get; set; }
    }

    public class MultiQueryApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public MultiQueryResponseList Response { get; set; }
    }
}
