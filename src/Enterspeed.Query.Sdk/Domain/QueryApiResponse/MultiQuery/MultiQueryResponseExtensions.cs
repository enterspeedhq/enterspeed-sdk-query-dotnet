using System.Collections.Generic;
using System.Linq;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse.MultiQuery
{
    /// <summary>
    /// Extension methods for converting MultiQueryResponse list to dictionary indexed by query name
    /// </summary>
    public static class MultiQueryResponseExtensions
    {
        /// <summary>
        /// Converts a list of MultiQueryResponse (from API) to a dictionary indexed by query name.
        /// The raw API responses are stored, and Get&lt;T&gt;() performs type conversion on demand.
        /// </summary>
        /// <param name="responses">The raw API responses</param>
        /// <returns>Dictionary keyed by query name, containing raw MultiQueryResponse objects</returns>
      public static Dictionary<string, MultiQueryResponse> ToMultiQueryResponse(
            this List<MultiQueryResponse> responses) =>
            responses?.ToDictionary(r => r.Name, r => r)
            ?? new Dictionary<string, MultiQueryResponse>();

    }
}
