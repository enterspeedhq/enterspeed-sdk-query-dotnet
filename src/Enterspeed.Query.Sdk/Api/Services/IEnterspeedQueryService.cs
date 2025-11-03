using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Api.Services
{
    public interface IEnterspeedQueryService
    {
        /// <summary>
        /// Creates a single query request.
        /// <a href="https://docs.enterspeed.com/api#tag/Query/operation/queryContentPost">Read more</a>
        /// </summary>
        /// <param name="apiKey">Api key to validate your environment. Example: environment-1637c4d0-e878-4738-b866-152106a4f88c</param>
        /// <param name="index">The alias of the index to query</param>
        /// <param name="query">Will be turned in to the request body when posted</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<QueryApiResponse> Query(string apiKey, string index, QueryObject query, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Creates a single query request.
        /// <a href="https://docs.enterspeed.com/api#tag/Query/operation/queryContentPost">Read more</a>
        /// </summary>
        /// <param name="apiKey">Api key to validate your environment. Example: environment-1637c4d0-e878-4738-b866-152106a4f88c</param>
        /// <param name="index">The alias of the index to query</param>
        /// <param name="query">Will be turned in to the request body when posted</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<QueryApiResponse<IContent>> QueryTyped(string apiKey, string index, QueryObject query, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Creates a multi query request.
        /// Note: The maximum queries in one request is limited to 5. <br/>
        /// <a href="https://docs.enterspeed.com/api#tag/Query/operation/queryMultiContentPost">Read more</a>
        /// </summary>
        /// <param name="apiKey">Api key to validate your environment. Example: environment-1637c4d0-e878-4738-b866-152106a4f88c</param>
        /// <param name="queries">Will be turned in to the request body when posted</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<MultiQueryApiResponse> Query(string apiKey, List<MultiQueryObject> queries, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Creates a multi query request.
        /// Note: The maximum queries in one request is limited to 5. <br/>
        /// <a href="https://docs.enterspeed.com/api#tag/Query/operation/queryMultiContentPost">Read more</a>
        /// </summary>
        /// <param name="apiKey">API key to validate your environment. Example: environment-1637c4d0-e878-4738-b866-152106a4f88c</param>
        /// <param name="queries">Will be turned in to the request body when posted</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<MultiQueryApiResponse<IContent>> QueryTyped(string apiKey, List<MultiQueryObject> queries, CancellationToken? cancellationToken = null);
    }
}