using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Api.Services
{
    public interface IEnterspeedQueryService
    {
        /// <summary>
        /// Creates a single query request with strongly-typed results.
        /// Returns a unified response with success/failure pattern.
        /// <a href="https://docs.enterspeed.com/api#tag/Query/operation/queryContentPost">Read more</a>
        /// </summary>
        /// <param name="apiKey">Api key to validate your environment. Example: environment-1637c4d0-e878-4738-b866-152106a4f88c</param>
        /// <param name="index">The alias of the index to query</param>
        /// <param name="query">Will be turned in to the request body when posted</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A QueryApiResponse containing either ISuccess&lt;Dictionary&lt;string, object&gt;&gt; or IFailure</returns>
        Task<QueryApiResponse<Dictionary<string, object>>> Query(string apiKey, string index, QueryObject query, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Creates a single query request with custom strongly-typed results.
        /// Returns a unified response with success/failure pattern.
        /// <a href="https://docs.enterspeed.com/api#tag/Query/operation/queryContentPost">Read more</a>
        /// </summary>
        /// <typeparam name="T">The expected type of the query results.</typeparam>
        /// <param name="apiKey">Api key to validate your environment. Example: environment-1637c4d0-e878-4738-b866-152106a4f88c</param>
        /// <param name="index">The alias of the index to query</param>
        /// <param name="query">Will be turned in to the request body when posted</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A QueryApiResponse containing either ISuccess&lt;T&gt; or IFailure</returns>
        Task<QueryApiResponse<T>> QueryTyped<T>(string apiKey, string index, QueryObject query, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Creates a multi query request.
        /// Note: The maximum queries in one request is limited to 5. <br/>
        /// <a href="https://docs.enterspeed.com/api#tag/Query/operation/queryMultiContentPost">Read more</a>
        /// </summary>
        /// <param name="apiKey">Api key to validate your environment. Example: environment-1637c4d0-e878-4738-b866-152106a4f88c</param>
        /// <param name="queries">Will be turned in to the request body when posted</param>
        /// <param name="cancellationToken"></param>
        /// <returns>A MultiQueryApiResponse containing individual success/failure responses per query</returns>
        Task<MultiQueryApiResponse> Query(string apiKey, List<MultiQueryObject> queries, CancellationToken? cancellationToken = null);
    }
}
