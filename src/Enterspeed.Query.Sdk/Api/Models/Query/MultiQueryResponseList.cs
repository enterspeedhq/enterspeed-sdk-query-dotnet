using System.Collections.Generic;
using System.Linq;
using Enterspeed.Query.Sdk.Api.Models.MultiQuery;

namespace Enterspeed.Query.Sdk.Api.Models.Query
{
    /// <summary>
    /// Internal wrapper for multi-query API responses.
    /// Provides access to success and error responses from the API.
    /// For typed access to query results, use MultiQueryApiResponse.Get&lt;T&gt;() instead.
    /// </summary>
    public class MultiQueryResponseList
    {
        private readonly List<MultiQueryResponse> _results;

        public MultiQueryResponseList()
        {
            _results = new List<MultiQueryResponse>();
        }

        public MultiQueryResponseList(List<MultiQueryResponse> results)
        {
            _results = results ?? new List<MultiQueryResponse>();
        }

        public IReadOnlyList<MultiQueryResponseSuccess> GetSuccessResponse() =>
            _results.OfType<MultiQueryResponseSuccess>().ToList().AsReadOnly();

        public IReadOnlyList<MultiQueryResponseError> GetErrorResponse() =>
            _results.OfType<MultiQueryResponseError>().ToList().AsReadOnly();
    }
}
