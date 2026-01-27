using System;
using System.Collections.Generic;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Domain.Builders
{
    /// <summary>
    /// Fluent builder for constructing multi-query requests.
    /// Allows adding multiple queries with unique keys that will be executed in a single API call.
    /// </summary>
    public class MultiQueryBuilder
    {
        private const int MaxQueriesPerRequest = 5;
        private readonly Dictionary<string, MultiQueryObject> _queries = new Dictionary<string, MultiQueryObject>();

        /// <summary>
        /// Gets the current number of queries in this builder.
        /// </summary>
        public int Count => _queries.Count;

        /// <summary>
        /// Adds a query using a fluent builder callback.
        /// </summary>
        /// <param name="key">Unique identifier for this query in the response.</param>
        /// <param name="index">The index alias to query against.</param>
        /// <param name="builderAction">Callback to configure the query using the fluent builder.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when key or index is null/empty, or key is duplicate.</exception>
        /// <exception cref="InvalidOperationException">Thrown when maximum query limit is exceeded.</exception>
        public MultiQueryBuilder AddQuery(string key, string index, Action<IQueryBuilder> builderAction)
        {
            ValidateKey(key);
            ValidateIndex(index);
            ValidateMaxQueries();

            if (builderAction == null)
                throw new ArgumentNullException(nameof(builderAction));

            var queryBuilder = new QueryBuilder();
            builderAction(queryBuilder);
            var queryObject = queryBuilder.Build();

            AddQueryInternal(key, index, queryObject);
            return this;
        }

        /// <summary>
        /// Adds a pre-constructed query object.
        /// </summary>
        /// <param name="key">Unique identifier for this query in the response.</param>
        /// <param name="index">The index alias to query against.</param>
        /// <param name="query">The pre-constructed query object.</param>
        /// <returns>This builder instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when key or index is null/empty, or key is duplicate.</exception>
        /// <exception cref="ArgumentNullException">Thrown when query is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when maximum query limit is exceeded.</exception>
        public MultiQueryBuilder AddQuery(string key, string index, QueryObject query)
        {
            ValidateKey(key);
            ValidateIndex(index);
            ValidateMaxQueries();

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            AddQueryInternal(key, index, query);
            return this;
        }

        /// <summary>
        /// Builds an immutable multi-query request containing all added queries.
        /// </summary>
        /// <returns>An immutable MultiQueryRequest ready for execution.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no queries have been added.</exception>
        public MultiQueryRequest Build()
        {
            if (_queries.Count == 0)
                throw new InvalidOperationException("Cannot build a multi-query request with no queries. Add at least one query before calling Build().");

            var queryList = new List<MultiQueryObject>(_queries.Values);
            return new MultiQueryRequest(queryList);
        }

        /// <summary>
        /// Checks if a query with the specified key has been added.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if a query with this key exists; otherwise false.</returns>
        public bool ContainsKey(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && _queries.ContainsKey(key);
        }

        private void AddQueryInternal(string key, string index, QueryObject query)
        {
            var multiQuery = new MultiQueryObject
            {
                Name = key,
                Index = index,
                Filters = query.Filters,
                Aliases = query.Aliases,
                Sort = query.Sort,
                Pagination = query.Pagination,
                Facets = query.Facets
            };

            _queries.Add(key, multiQuery);
        }

        private void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Query key cannot be null or empty.", nameof(key));

            if (_queries.ContainsKey(key))
                throw new ArgumentException($"A query with the key '{key}' has already been added. Each query must have a unique key.", nameof(key));
        }

        private void ValidateIndex(string index)
        {
            if (string.IsNullOrWhiteSpace(index))
                throw new ArgumentException("Index cannot be null or empty.", nameof(index));
        }

        private void ValidateMaxQueries()
        {
            if (_queries.Count >= MaxQueriesPerRequest)
                throw new InvalidOperationException($"Cannot add more than {MaxQueriesPerRequest} queries to a single multi-query request. This is a limit imposed by the Enterspeed Query API.");
        }
    }
}

/*
 * Usage Example:
 *
 * var request = new MultiQueryBuilder()
 *     .AddQuery("users", "user-index", builder => builder
 *         .WithPagination(0, 5)
 *         .SortBy("updatedAt", SortOrder.Desc)
 *         .Where(new EqualsOperator { Field = "status", Value = "active" }))
 *     .AddQuery("products", "product-index", new QueryObject
 *     {
 *         Pagination = new Pagination { Page = 0, PageSize = 10 }
 *     })
 *     .Build();
 */

