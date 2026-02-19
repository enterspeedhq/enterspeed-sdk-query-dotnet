using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Non-generic interface for successful responses, enabling pattern matching without knowing T.
    /// </summary>
    public interface ISuccess : IResponse
    {
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
    /// Represents a successful query response with strongly-typed data.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    public interface ISuccess<T> : ISuccess, IResponse<T>
    {
        /// <summary>
        /// Gets the strongly-typed value from the successful response.
        /// </summary>
        /// <returns>The first result or default.</returns>
        T Value();

        /// <summary>
        /// Gets the list of results from the successful response.
        /// </summary>
        IReadOnlyList<T> Results { get; }

        // Inherits TotalResults and Facets from ISuccess
    }
}
