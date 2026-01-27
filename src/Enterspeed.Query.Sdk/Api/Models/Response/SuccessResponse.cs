using System.Collections.Generic;
using System.Linq;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Implementation of ISuccess for successful query responses.
    /// Exposes strongly-typed data and does not expose errors.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    public class SuccessResponse<T> : ISuccess<T>
    {
        private readonly List<T> _results;
        private readonly List<FacetResult> _facets;

        public SuccessResponse(List<T> results, int totalResults, List<FacetResult> facets = null)
        {
            _results = results ?? new List<T>();
            TotalResults = totalResults;
            _facets = facets ?? new List<FacetResult>();
        }

        /// <inheritdoc />
        public bool Status => true;

        /// <inheritdoc />
        public IReadOnlyList<T> Results => _results.AsReadOnly();

        /// <inheritdoc />
        public int TotalResults { get; }

        /// <inheritdoc />
        public IReadOnlyList<FacetResult> Facets => _facets.AsReadOnly();

        /// <inheritdoc />
        public T Value()
        {
            return _results.FirstOrDefault();
        }
    }
}
