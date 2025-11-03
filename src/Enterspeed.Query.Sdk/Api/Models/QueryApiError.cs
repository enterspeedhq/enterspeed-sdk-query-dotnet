using System.Net;
using System.Net.Http.Headers;

namespace Enterspeed.Query.Sdk.Api.Models
{
    public class QueryApiError
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public string[] Errors { get; set; }
    }
}