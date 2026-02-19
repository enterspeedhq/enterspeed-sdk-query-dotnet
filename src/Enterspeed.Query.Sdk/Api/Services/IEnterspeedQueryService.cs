using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Api.Services
{
    public interface IEnterspeedQueryService
    {
        /// <summary>
        /// Creates a query request.
        /// Note: The maximum queries in one request is limited to 5. <br/>
        /// <a href="https://docs.enterspeed.com/api#tag/Query/operation/queryMultiContentPost">Read more</a>
        /// </summary>
        /// <param name="apiKey">Api key to validate your environment. Example: environment-1637c4d0-e878-4738-b866-152106a4f88c</param>
        /// <param name="queries">Will be turned in to the request body when posted</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A MultiQueryApiResponse containing individual success/failure responses per query</returns>
        Task<MultiQueryApiResponse> Query(string apiKey, QueryRequest queries, CancellationToken? cancellationToken = null);
    }
}
