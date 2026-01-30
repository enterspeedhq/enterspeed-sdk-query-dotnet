using System.Collections.Generic;
using System.Linq;
using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse
{
    /// <summary>
    /// Extension methods for mapping QueryResponseError to QueryError
    /// </summary>
    public static class QueryResponseErrorExtensions
    {
        /// <summary>
        /// Converts a QueryResponseError to a QueryError with the message and errors array
        /// </summary>
        /// <typeparam name="T">The type of the query response</typeparam>
        /// <param name="errorResponse">The error response from the API</param>
        /// <param name="index">Optional index name for the query</param>
        /// <returns>A QueryError containing the message and errors</returns>
        public static QueryError ToQueryError<T>(this QueryResponseError<T> errorResponse, string index = null)
        {
            return new QueryError
            {
                Index = index,
                Message = errorResponse.Message ?? "Query failed",
                Errors = errorResponse.Errors
            };
        }

        /// <summary>
        /// Converts a collection of QueryResponseError to a list of QueryError objects
        /// </summary>
        /// <typeparam name="T">The type of the query response</typeparam>
        /// <param name="errorResponses">The collection of error responses from the API</param>
        /// <param name="index">Optional index name for the queries</param>
        /// <returns>A list of QueryError objects</returns>
        public static List<QueryError> ToQueryErrors<T>(this IEnumerable<QueryResponseError<T>> errorResponses, string index = null)
        {
            return errorResponses.Select(e => e.ToQueryError(index)).ToList();
        }
    }
}
