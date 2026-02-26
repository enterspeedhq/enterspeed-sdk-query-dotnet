using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Domain.Builders.Filter;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Domain.Builders.Query
{
    /// <summary>
    /// Generic implementation of IQueryBuilder with implicit entity type.
    /// Wraps the non-generic QueryBuilder and provides strongly-typed method overloads.
    /// </summary>
    /// <typeparam name="T">The entity type for this query builder.</typeparam>
    internal class Query<T> : IQuery<T>
    {
        private readonly Query _inner;

        public Query()
        {
            _inner = new Query();
        }

        internal Query(Query inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        // Explicit implementations of IQueryBuilder (non-generic interface)
        IQuery IQuery.WithPagination(int page, int pageSize)
        {
            _inner.WithPagination(page, pageSize);
            return this;
        }

        IQuery IQuery.WithPagination(Action<Pagination> configure)
        {
            _inner.WithPagination(configure);
            return this;
        }

        IQuery IQuery.SortBy(string field, SortOrder order)
        {
            _inner.SortBy(field, order);
            return this;
        }

        IQuery IQuery.SortBy<TEntity>(Expression<Func<TEntity, object>> fieldSelector, SortOrder order)
        {

            return _inner.SortBy(fieldSelector, order);
        }

        IQuery IQuery.Where(Action<IFilterBuilder> configure)
        {
            return _inner.Where(configure);
        }

        IQuery IQuery.Where(IOperator filter)
        {
            return _inner.Where(filter);
        }

        IQuery IQuery.WhereAll(params IOperator[] filters)
        {
            return _inner.WhereAll(filters);
        }

        IQuery IQuery.WithFacet(string field, string name, int size)
        {
            _inner.WithFacet(field, name, size);
            return this;
        }

        IQuery IQuery.WithAliases(params string[] aliases)
        {
            _inner.WithAliases(aliases);
            return this;
        }

        public QueryObject Build()
        {
            return _inner.Build();
        }

        // Public implementations of IQueryBuilder<T> (generic interface)
        public IQuery<T> WithPagination(int page, int pageSize)
        {
            _inner.WithPagination(page, pageSize);
            return this;
        }

        public IQuery<T> WithPagination(Action<Pagination> configure)
        {
            _inner.WithPagination(configure);
            return this;
        }

        public IQuery<T> SortBy(string field, SortOrder order = SortOrder.Asc)
        {
            _inner.SortBy(field, order);
            return this;
        }

        public IQuery<T> SortBy(Expression<Func<T, object>> fieldSelector, SortOrder order = SortOrder.Asc)
        {
            var fieldName = GetFieldName(fieldSelector);

            _inner.SortBy(fieldName, order);
            return this;
        }

        public IQuery<T> Where(Action<IFilterBuilder<T>> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var filterBuilder = new FilterBuilder<T>();
            configure(filterBuilder);

            var filters = filterBuilder.BuildFilters();
            if (filters.Count > 0)
            {
                _inner.WhereAll(filters.ToArray());
            }

            return this;
        }

        public IQuery<T> WithFacet(string field, string name = null, int size = 10)
        {
            _inner.WithFacet(ToCamelCase(field), name, size);
            return this;
        }

        public IQuery<T> WithFacet(Expression<Func<T, object>> fieldSelector, string name = null, int size = 10)
        {
            if (fieldSelector == null)
            {
                throw new ArgumentNullException(nameof(fieldSelector));
            }

            var fieldName = GetFieldName(fieldSelector);

            _inner.WithFacet(fieldName, name, size);
            return this;
        }


        public IQuery<T> WithAliases(params string[] aliases)
        {
            _inner.WithAliases(aliases);
            return this;
        }

        private static string ToCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
            {
                return value;
            }

            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        private static string GetFieldName(Expression<Func<T, object>> fieldSelector)
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
    }
}
