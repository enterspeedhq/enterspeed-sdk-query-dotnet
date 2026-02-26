using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse.MultiQuery;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Response wrapper for multi-query API calls.
    /// Wraps the multi-query response list with HTTP metadata.
    /// </summary>
    public class MultiQueryApiResponse
    {
        private readonly Dictionary<(string QueryName, System.Type TargetType), object> _cachedResponses = new Dictionary<(string, System.Type), object>();

        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// Raw API responses indexed by query name.
        /// Each response contains the original data from the API.
        /// Use Get&lt;T&gt;(queryName) to retrieve and convert to typed results.
        /// </summary>
        internal Dictionary<string, MultiQueryResponse> Response { get; set; }

        /// <summary>
        /// Retrieves a strongly-typed response for a specific query by name.
        /// Converts the raw API response to IResponse&lt;T&gt; on-demand.
        /// </summary>
        public IResponse<T> Get<T>(string queryName)
        {
            if (string.IsNullOrWhiteSpace(queryName))
            {
                return new ErrorResponse<T>(new QueryError { Message = "Query name cannot be null or empty", } );
            }

            if (Response is null)
            {
                return new ErrorResponse<T>(new QueryError { Message = "Response dictionary is not initialized" });
            }

            // Check if query exists
            if (!Response.TryGetValue(queryName, out var apiResponse))
            {
                return new ErrorResponse<T>(new QueryError { Message = $"Query '{queryName}' not found in response" });
            }

            // Check cache for typed conversion
            var cacheKey = (queryName, typeof(T));
            if (_cachedResponses.TryGetValue(cacheKey, out var cached))
            {
                return cached as IResponse<T>;
            }

            // Handle error responses
            if (apiResponse is MultiQueryResponseError error)
            {
                var queryError = new QueryError
                {
                    Index = error.Index,
                    Name = error.Name,
                    Message = error.Message,
                    Errors = error.Errors
                };

                var failure = new ErrorResponse<T>(queryError);
                _cachedResponses[cacheKey] = failure;

                return failure;
            }

            // Handle success responses - convert to typed results
            if (apiResponse is MultiQueryResponseSuccess success)
            {
                try
                {
                    var serializer = new SystemTextJsonSerializer(); // TODO Consider injecting serializer via constructor for flexibility
                    var typedResults = success.Results
                        .Select(dict => serializer.Serialize(dict))
                        .Select(json => serializer.Deserialize<T>(json))
                        .Where(item => item != null)
                        .ToList();

                    var typedSuccessResponse = new QueryResponseSuccess<T>
                    {
                        TotalResults = success.TotalResults,
                        Results = typedResults,
                        Facets = success.Facets?.ToList() ?? new List<FacetResult>()
                    };

                    var typedSuccess = new SuccessResponse<T>(typedSuccessResponse);
                    _cachedResponses[cacheKey] = typedSuccess;
                    return typedSuccess;
                }
                catch (System.Exception ex)
                {
                    var failure = new ErrorResponse<T>(new QueryError
                    {
                        Message = $"Failed to convert query '{queryName}' to type {typeof(T).Name}: {ex.Message}"
                    });
                    // Do not cache the failure to avoid no being able to query the right and type after usage the first time with the wrong type.
                    // We can consider caching the failure with the specific type to avoid trying to convert to the same wrong type again,
                    // but we should still be able to try with another type after a failure with a specific type.
                    return failure;
                }
            }

            // Unknown response type
            return new ErrorResponse<T>(new QueryError { Message = "Unknown response type" });
        }

        /// <summary>
        /// Checks if a query with the specified name exists in the response.
        /// </summary>
        public bool ContainsQuery(string queryName) =>
            !string.IsNullOrWhiteSpace(queryName)
            && Response != null
            && Response.ContainsKey(queryName);

        /// <summary>
        /// Gets all query names present in the response.
        /// </summary>
        public IReadOnlyList<string> GetQueryNames() => Response == null
            ? new List<string>().AsReadOnly()
            : Response.Keys.ToList().AsReadOnly();
    }
}
