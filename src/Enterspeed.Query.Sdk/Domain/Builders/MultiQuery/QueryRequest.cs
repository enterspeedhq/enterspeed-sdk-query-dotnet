using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Domain.Builders.MultiQuery
{
    /// <summary>
    /// Immutable container for a multi-query request.
    /// Contains all queries to be executed in a single API call.
    /// </summary>
    public class QueryRequest
    {
        /// <summary>
        /// Gets the read-only list of query items.
        /// </summary>
        public IReadOnlyList<MultiQueryObject> Queries { get; }

        /// <summary>
        /// Initializes a new instance of the MultiQueryRequest class.
        /// </summary>
        /// <param name="queries">The collection of queries to include in this request.</param>
        internal QueryRequest(IList<MultiQueryObject> queries)
        {
            // Defensive copy to avoid external mutations affecting this request
            var snapshot = new List<MultiQueryObject>(queries);
            Queries = new ReadOnlyCollection<MultiQueryObject>(snapshot);
        }
    }
}

