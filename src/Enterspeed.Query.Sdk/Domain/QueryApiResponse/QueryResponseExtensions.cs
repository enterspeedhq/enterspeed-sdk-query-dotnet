using System.Collections.Generic;
using System.Linq;

namespace Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query
{
    /// <summary>
    /// Extension methods for converting QueryResponse list to dictionary indexed by query name
    /// </summary>
    public static class QueryResponseExtensions
    {
        /// <summary>
        /// Converts a list of QueryResponse (from API) to a dictionary indexed by query name.
        /// The raw API responses are stored, and Get<T>() performs type conversion on demand.
        /// </summary>
        /// <param name="responses">The raw API responses</param>
        /// <returns>Dictionary keyed by query name, containing raw QueryResponse objects</returns>
        public static Dictionary<string, QueryResponse> ToQueryResponse(
            this List<QueryResponse> responses) =>
            responses?.ToDictionary(r => r.Name, r => r)
            ?? new Dictionary<string, QueryResponse>();
    }
}
