using System.Collections.Generic;
using System.Linq;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query;

namespace Enterspeed.Query.Sdk.Api.Models.Response
{
    /// <summary>
    /// Wraps QueryResponseSuccess from the API for SDK consumers.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    public class SuccessResponse<T> : ISuccess<T>
    {
        private readonly QueryResponseSuccess<T> _apiResponse;

        public SuccessResponse(QueryResponseSuccess<T> apiResponse)
        {
            _apiResponse = apiResponse ?? throw new System.ArgumentNullException(nameof(apiResponse));
        }

        internal SuccessResponse(List<T> results, int totalResults, List<FacetResult> facets = null)
        {
            _apiResponse = new QueryResponseSuccess<T>
            {
                Results = results ?? new List<T>(),
                TotalResults = totalResults,
                Facets = facets ?? new List<FacetResult>()
            };
        }

        public bool Status => true;

        public T Value()
        {
            return _apiResponse.Results.FirstOrDefault();
        }

        public IReadOnlyList<T> Results => _apiResponse.Results?.AsReadOnly() ?? new List<T>().AsReadOnly();

        public int TotalResults => _apiResponse.TotalResults;

        public IReadOnlyList<FacetResult> Facets => _apiResponse.Facets?.AsReadOnly() ?? new List<FacetResult>().AsReadOnly();
    }
}
