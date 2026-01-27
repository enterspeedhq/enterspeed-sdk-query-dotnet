using System;
using System.Collections.Generic;
using System.Linq;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.LogicalOperators;

namespace Enterspeed.Query.Sdk.Domain.Builders
{
    /// <summary>
    /// Concrete implementation of IQueryBuilder for building individual queries.
    /// Accumulates configuration and produces a QueryObject when Build() is called.
    /// </summary>
    internal class QueryBuilder : IQueryBuilder
    {
        private Pagination _pagination;
        private readonly List<Sort> _sorts = new List<Sort>();
        private readonly List<IOperator> _accumulatedFilters = new List<IOperator>();
        private readonly List<Facet> _facets = new List<Facet>();
        private readonly List<string> _aliases = new List<string>();

        public IQueryBuilder WithPagination(int page, int pageSize)
        {
            if (page < 0)
            {
                throw new ArgumentException("Page number must be non-negative.", nameof(page));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentException("Page size must be positive.", nameof(pageSize));
            }

            _pagination = new Pagination { Page = page, PageSize = pageSize };
            return this;
        }

        public IQueryBuilder SortBy(string field, SortOrder order = SortOrder.Asc)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                throw new ArgumentException("Field name cannot be null or whitespace.", nameof(field));
            }

            _sorts.Add(new Sort { Field = field, Order = order });
            return this;
        }

        public IQueryBuilder Where(Action<IFilterBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var filterBuilder = new FilterBuilder();
            configure(filterBuilder);

            var filters = filterBuilder.BuildFilters();
            if (filters.Count > 0)
            {
                _accumulatedFilters.AddRange(filters);
            }

            return this;
        }

        public IQueryBuilder Where(IOperator filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            _accumulatedFilters.Add(filter);
            return this;
        }

        public IQueryBuilder WhereAll(params IOperator[] filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            switch (filters.Length)
            {
                case 0:
                    break;
                case 1:
                    _accumulatedFilters.Add(filters[0]);
                    break;
                default:
                    _accumulatedFilters.Add(new AndOperator { And = filters.ToList() });
                    break;
            }

            return this;
        }

        public IQueryBuilder WithFacet(string field, string name = null, int size = 10)
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                throw new ArgumentException("Field name cannot be null or whitespace.", nameof(field));
            }

            if (size <= 0)
            {
                throw new ArgumentException("Facet size must be positive.", nameof(size));
            }

            _facets.Add(new Facet
            {
                Field = field,
                Name = name ?? "",
                Size = size
            });

            return this;
        }

        public IQueryBuilder WithAliases(params string[] aliases)
        {
            if (aliases == null)
            {
                throw new ArgumentNullException(nameof(aliases));
            }

            _aliases.AddRange(aliases.Where(a => !string.IsNullOrWhiteSpace(a)));
            return this;
        }

        public QueryObject Build()
        {
            var queryObject = new QueryObject();

            // Set pagination
            if (_pagination != null)
            {
                queryObject.Pagination = _pagination;
            }

            // Set sorts
            if (_sorts.Count > 0)
            {
                queryObject.Sort = _sorts;
            }

            // Combine accumulated filters with AND logic (matches Enterspeed Query API structure)
            if (_accumulatedFilters.Count > 0)
            {
                queryObject.Filters = new AndOperator { And = _accumulatedFilters };
            }

            // Set facets
            if (_facets.Count > 0)
            {
                queryObject.Facets = _facets;
            }

            // Set aliases
            if (_aliases.Count > 0)
            {
                queryObject.Aliases = _aliases;
            }

            return queryObject;
        }
    }
}
