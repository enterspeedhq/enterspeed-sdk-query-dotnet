using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using Enterspeed.Query.Sdk.Api.Models.Query;
using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Response wrapper for single query API calls.
    /// </summary>
    public class QueryApiResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public IQueryResponse Response { get; set; }
    }

    /// <summary>
    /// Response wrapper for single typed query API calls.
    /// </summary>
    /// <typeparam name="T">The expected type of the query results.</typeparam>
    public class QueryApiResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }
        public IQueryResponse<T> Response { get; set; }
    }

    /// <summary>
    /// Response container for multi-query API calls.
    /// Provides access to individual query responses by query name.
    /// Supports partial failures - one query can fail without affecting others.
    /// </summary>
    public class MultiQueryApiResponse
    {
        private readonly IJsonSerializer _serializer;
        private readonly Dictionary<string, object> _cachedResponses;

        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseHeaders Headers { get; set; }
        public string Message { get; set; }

        /// <summary>
        /// The raw response list from the API.
        /// </summary>
        public MultiQueryResponseList Response { get; set; }

        public MultiQueryApiResponse()
        {
            _serializer = new SystemTextJsonSerializer();
            _cachedResponses = new Dictionary<string, object>();
        }

        /// <summary>
        /// Retrieves a strongly-typed response for a specific query by name.
        /// Returns ISuccess&lt;T&gt; if the query succeeded and type matches.
        /// Returns IFailure if the query failed, key is missing, or type mismatch occurs.
        /// </summary>
        /// <typeparam name="T">The expected type of the query results.</typeparam>
        /// <param name="queryName">The unique key used when adding the query to the multi-query builder.</param>
        /// <returns>An IResponse&lt;T&gt; that is either ISuccess&lt;T&gt; or IFailure.</returns>
        public IResponse<T> Get<T>(string queryName)
        {
            if (string.IsNullOrWhiteSpace(queryName))
            {
                return CreateFailureResponse<T>(new QueryError(
                    "Query name cannot be null or empty",
                    "INVALID_QUERY_NAME"
                ));
            }

            if (Response == null)
            {
                return CreateFailureResponse<T>(new QueryError(
                    "Response list is not initialized",
                    "NO_RESPONSE"
                ));
            }

            // Check cache first
            var cacheKey = $"{queryName}_{typeof(T).FullName}";
            if (_cachedResponses.TryGetValue(cacheKey, out var cached))
            {
                return cached as IResponse<T>;
            }

            // Look for error response first
            var errorResponse = Response.GetErrorResponse()
                .FirstOrDefault(x => x.Name == queryName);

            if (errorResponse != null)
            {
                var errors = ConvertToQueryErrors(errorResponse);
                var failure = CreateFailureResponse<T>(errors);
                _cachedResponses[cacheKey] = failure;
                return failure;
            }

            // Look for success response
            var successResponse = Response.GetSuccessResponse()
                .FirstOrDefault(x => x.Name == queryName);

            if (successResponse == null)
            {
                var failure = CreateFailureResponse<T>(new QueryError(
                    $"Query '{queryName}' not found in response",
                    "QUERY_NOT_FOUND"
                ));
                _cachedResponses[cacheKey] = failure;
                return failure;
            }

            // Attempt to deserialize to the requested type
            try
            {
                var typedResults = new List<T>();
                foreach (var dict in successResponse.Results)
                {
                    var serialized = _serializer.Serialize(dict);
                    var deserialized = _serializer.Deserialize<T>(serialized);
                    if (deserialized != null)
                    {
                        typedResults.Add(deserialized);
                    }
                }

                var success = new SuccessResponse<T>(
                    typedResults,
                    successResponse.TotalResults,
                    successResponse.Facets
                );

                _cachedResponses[cacheKey] = success;
                return success;
            }
            catch (Exception ex)
            {
                var failure = CreateFailureResponse<T>(new QueryError(
                    $"Failed to deserialize query '{queryName}' to type {typeof(T).Name}: {ex.Message}",
                    "TYPE_MISMATCH"
                ));
                _cachedResponses[cacheKey] = failure;
                return failure;
            }
        }

        /// <summary>
        /// Checks if a query with the specified name exists in the response.
        /// </summary>
        /// <param name="queryName">The query name to check.</param>
        /// <returns>True if the query exists (either as success or error); otherwise false.</returns>
        public bool ContainsQuery(string queryName)
        {
            if (string.IsNullOrWhiteSpace(queryName) || Response == null)
                return false;

            return Response.GetSuccessResponse().Any(x => x.Name == queryName) ||
                   Response.GetErrorResponse().Any(x => x.Name == queryName);
        }

        /// <summary>
        /// Gets all query names present in the response.
        /// </summary>
        /// <returns>A list of all query names.</returns>
        public IReadOnlyList<string> GetQueryNames()
        {
            if (Response == null)
                return new List<string>().AsReadOnly();

            var names = new List<string>();
            names.AddRange(Response.GetSuccessResponse().Select(x => x.Name));
            names.AddRange(Response.GetErrorResponse().Select(x => x.Name));
            return names.Distinct().ToList().AsReadOnly();
        }

        private IResponse<T> CreateFailureResponse<T>(QueryError error)
        {
            return new FailureResponseTyped<T>(new List<QueryError> { error });
        }

        private IResponse<T> CreateFailureResponse<T>(List<QueryError> errors)
        {
            return new FailureResponseTyped<T>(errors);
        }

        private List<QueryError> ConvertToQueryErrors(MultiQueryResponseError errorResponse)
        {
            var errors = new List<QueryError>();

            if (!string.IsNullOrWhiteSpace(errorResponse.Message))
            {
                errors.Add(new QueryError(errorResponse.Message));
            }

            if (errorResponse.Errors != null && errorResponse.Errors.Length > 0)
            {
                errors.AddRange(errorResponse.Errors.Select(err => new QueryError(err)));
            }

            return errors.Count > 0 ? errors : new List<QueryError> { new QueryError("Unknown error") };
        }
    }
}
