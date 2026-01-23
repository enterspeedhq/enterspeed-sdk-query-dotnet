using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Base interface for all query responses.
    /// Provides a common Status property to determine success or failure.
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// Indicates whether the query was successful.
        /// </summary>
        bool Status { get; }
    }

    /// <summary>
    /// Base interface for typed query responses.
    /// </summary>
    /// <typeparam name="T">The expected type of the response data.</typeparam>
    public interface IResponse<T> : IResponse
    {
    }

    /// <summary>
    /// Represents a successful query response with strongly-typed data.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    public interface ISuccess<T> : IResponse<T>
    {
        /// <summary>
        /// Gets the strongly-typed value from the successful response.
        /// </summary>
        /// <returns>The deserialized response data.</returns>
        T Value();

        /// <summary>
        /// Gets the list of results from the successful response.
        /// </summary>
        IReadOnlyList<T> Results { get; }

        /// <summary>
        /// Gets the total number of results matching the query.
        /// </summary>
        int TotalResults { get; }

        /// <summary>
        /// Gets the facet results if faceting was requested.
        /// </summary>
        IReadOnlyList<FacetResult> Facets { get; }
    }

    /// <summary>
    /// Represents a failed query response with error information.
    /// Does not expose a Value() method or typed data.
    /// </summary>
    public interface IFailure : IResponse
    {
        /// <summary>
        /// Gets the collection of errors that caused the query to fail.
        /// </summary>
        IReadOnlyList<QueryError> Errors { get; }
    }
}
