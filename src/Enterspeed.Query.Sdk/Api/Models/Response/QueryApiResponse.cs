using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{

    // We should be able to use non-generic version for simple use-cases
    /// <summary>
    /// Response wrapper for single query API calls (non-generic version).
    /// Wraps the API response with HTTP metadata.
    /// Results are returned as Dictionary&lt;string, object&gt;.
    /// </summary>
    public class QueryApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// The SDK response (ISuccess&lt;Dictionary&lt;string, object&gt;&gt; or IFailure).
        /// Use pattern matching: if (result.Response is ISuccess&lt;Dictionary&lt;string, object&gt;&gt; success) { ... }
        /// </summary>
        public IResponse<Dictionary<string, object>> Response { get; set; }

        public bool IsSuccess => Response is ISuccess<Dictionary<string, object>>;
    }

    /// <summary>
    /// Response wrapper for single query API calls.
    /// Wraps the API response with HTTP metadata.
    /// </summary>
    /// <typeparam name="T">The expected type of the query results.</typeparam>
    public class QueryApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// The SDK response (ISuccess&lt;T&gt; or IFailure).
        /// </summary>
        /// <remarks>
        /// Use pattern matching to inspect the result:
        /// <code>
        /// if (result.Response is ISuccess&lt;T&gt; success)
        /// {
        ///     // Handle success
        /// }
        /// else if (result.Response is IFailure failure)
        /// {
        ///     // Handle failure
        /// }
        /// </code>
        /// </remarks>
        public IResponse<T> Response { get; set; }

        /// <summary>
        /// Checks if the query was successful.
        /// </summary>
        public bool IsSuccess => Response is ISuccess<T>;
    }

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
        /// Checks if all queries were successful.
        /// </summary>
        public bool IsSuccess => Response != null && Response.Values.All(r => r.Status == QueryStatus.Success);

        /// <summary>
        /// Retrieves a strongly-typed response for a specific query by name.
        /// Converts the raw API response to IResponse&lt;T&gt; on-demand.
        /// </summary>
        public IResponse<T> Get<T>(string queryName)
        {
            if (string.IsNullOrWhiteSpace(queryName))
            {
                return new FailureResponse<T>(new QueryError { Message = "Query name cannot be null or empty" });
            }

            if (Response == null) // TODO: Should this not be part of constructor validation? And implement constructor?
            {
                return new FailureResponse<T>(new QueryError { Message = "Response dictionary is not initialized" });
            }

            // Check if query exists
            if (!Response.TryGetValue(queryName, out var apiResponse))
            {
                return new FailureResponse<T>(new QueryError { Message = $"Query '{queryName}' not found in response" });
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
                    Message = error.Message ?? "Query failed",
                    Errors = error.Errors
                };
                var failure = new FailureResponse<T>(queryError);
                _cachedResponses[cacheKey] = failure;
                return failure;
            }

            // Handle success responses - convert to typed results
            if (apiResponse is MultiQueryResponseSuccess success)
            {
                try
                {
                    var serializer = new SystemTextJsonSerializer();
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
                    var failure = new FailureResponse<T>(new QueryError
                    {
                        Message = $"Failed to convert query '{queryName}' to type {typeof(T).Name}: {ex.Message}"
                    });
                    _cachedResponses[cacheKey] = failure;
                    return failure;
                }
            }

            // Unknown response type
            return new FailureResponse<T>(new QueryError { Message = "Unknown response type" });
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
