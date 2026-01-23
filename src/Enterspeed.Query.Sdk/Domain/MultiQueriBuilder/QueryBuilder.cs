using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
using Enterspeed.Query.Sdk.Domain.Models.LogicalOperators;

namespace Enterspeed.Query.Sdk.Domain.MultiQueriBuilder
{
    /// <summary>
    /// Fluent builder for constructing query objects.
    /// </summary>
    public class QueryBuilder : IQueryBuilder
    {
        private readonly List<FilterOperator> _filters = new List<FilterOperator>();
        private readonly List<Sort> _sorts = new List<Sort>();
        private readonly List<Facet> _facets = new List<Facet>();
        private readonly List<string> _aliases = new List<string>();
        private Pagination _pagination;

        /// <inheritdoc />
        public IQueryBuilder WithPagination(int page, int pageSize)
        {
            if (page < 0)
                throw new ArgumentException("Page must be non-negative.", nameof(page));

            if (pageSize < 1)
                throw new ArgumentException("Page size must be at least 1.", nameof(pageSize));

            _pagination = new Pagination
            {
                Page = page,
                PageSize = pageSize
            };

            return this;
        }

        /// <inheritdoc />
        public IQueryBuilder SortBy(string field, SortOrder order = SortOrder.Asc)
        {
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentException("Field cannot be null or empty.", nameof(field));

            _sorts.Add(new Sort
            {
                Field = field,
                Order = order
            });

            return this;
        }

        /// <inheritdoc />
        public IQueryBuilder Where(FilterOperator filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            _filters.Add(filter);
            return this;
        }

        /// <inheritdoc />
        public IQueryBuilder WhereAll(params FilterOperator[] filters)
        {
            if (filters == null || filters.Length == 0)
                throw new ArgumentException("At least one filter must be provided.", nameof(filters));

            foreach (var filter in filters)
            {
                if (filter == null)
                    throw new ArgumentNullException(nameof(filters), "Filter collection cannot contain null values.");

                _filters.Add(filter);
            }

            return this;
        }

        /// <inheritdoc />
        public IQueryBuilder WithFacet(string field, string name = null, int size = 10)
        {
            if (string.IsNullOrWhiteSpace(field))
                throw new ArgumentException("Field cannot be null or empty.", nameof(field));

            if (size < 1)
                throw new ArgumentException("Facet size must be at least 1.", nameof(size));

            _facets.Add(new Facet
            {
                Field = field,
                Name = name ?? field,
                Size = size
            });

            return this;
        }

        /// <inheritdoc />
        public IQueryBuilder WithAliases(params string[] aliases)
        {
            if (aliases == null || aliases.Length == 0)
                throw new ArgumentException("At least one alias must be provided.", nameof(aliases));

            foreach (var alias in aliases)
            {
                if (string.IsNullOrWhiteSpace(alias))
                    throw new ArgumentException("Alias cannot be null or empty.", nameof(aliases));

                _aliases.Add(alias);
            }

            return this;
        }

        /// <inheritdoc />
        public QueryObject Build()
        {
            var queryObject = new QueryObject
            {
                Pagination = _pagination,
                Sort = _sorts.Count > 0 ? _sorts : null,
                Facets = _facets.Count > 0 ? _facets : null,
                Aliases = _aliases.Count > 0 ? _aliases : null
            };

            // Build filters with AND logic if multiple filters exist
            if (_filters.Count > 0)
            {
                queryObject.Filters = new AndOperator
                {
                    And = _filters.Cast<IOperator>().ToList()
                };
            }

            return queryObject;
        }
    }
}
