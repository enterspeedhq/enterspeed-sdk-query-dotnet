using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using System.Reflection;
using Enterspeed.Query.Sdk.Domain.Builders.Filter;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.LogicalOperators;

namespace Enterspeed.Query.Sdk.Domain.Builders.Query
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

        public IQueryBuilder WithPagination(Action<Pagination> configure)
        {
            var pagination = new Pagination();
            configure(pagination);
            _pagination = pagination;
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

        public IQueryBuilder SortBy<T>(Expression<Func<T, object>> fieldSelector, SortOrder order = SortOrder.Asc)
        {
            var fieldName = GetFieldName(fieldSelector);

            _sorts.Add(new Sort
            {
                Field = fieldName,
                Order = order
            });
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
                Name = name ?? field,
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

        private static string GetFieldName<T>(Expression<Func<T, object>> fieldSelector)
        {
            // Extract field name from expression, respecting [JsonPropertyName]
            var body = fieldSelector.Body;
            if (body is UnaryExpression unary)
            {
                body = unary.Operand;
            }

            if (!(body is MemberExpression member))
            {
                throw new ArgumentException("Expression must be a member access expression (e.g., p => p.PropertyName)", nameof(fieldSelector));
            }

            var propertyInfo = member.Member as PropertyInfo;
            string fieldName;
            if (propertyInfo != null)
            {
                var jsonPropertyNameAttr = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jsonPropertyNameAttr != null && !string.IsNullOrWhiteSpace(jsonPropertyNameAttr.Name))
                {
                    fieldName = jsonPropertyNameAttr.Name;
                }
                else
                {
                    fieldName = ToCamelCase(propertyInfo.Name);
                }
            }
            else
            {
                fieldName = ToCamelCase(member.Member.Name);
            }

            return fieldName;
        }

        private static string ToCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
            {
                return value;
            }

            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }
    }
}
