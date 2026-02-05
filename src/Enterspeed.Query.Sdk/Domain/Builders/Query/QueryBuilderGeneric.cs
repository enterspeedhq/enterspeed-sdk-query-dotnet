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
    internal class QueryBuilder<T> : IQueryBuilder<T>
    {
        private readonly QueryBuilder _innerBuilder;

        public QueryBuilder()
        {
            _innerBuilder = new QueryBuilder();
        }

        internal QueryBuilder(QueryBuilder innerBuilder)
        {
            _innerBuilder = innerBuilder ?? throw new ArgumentNullException(nameof(innerBuilder));
        }

        // Explicit implementations of IQueryBuilder (non-generic interface)
        IQueryBuilder IQueryBuilder.WithPagination(int page, int pageSize)
        {
            _innerBuilder.WithPagination(page, pageSize);
            return this;
        }

        IQueryBuilder IQueryBuilder.WithPagination(Action<Pagination> configure)
        {
            _innerBuilder.WithPagination(configure);
            return this;
        }

        IQueryBuilder IQueryBuilder.SortBy(string field, SortOrder order)
        {
            _innerBuilder.SortBy(field, order);
            return this;
        }

        IQueryBuilder IQueryBuilder.SortBy<TEntity>(Expression<Func<TEntity, object>> fieldSelector, SortOrder order)
        {

            return _innerBuilder.SortBy(fieldSelector, order);
        }

        IQueryBuilder IQueryBuilder.Where(Action<IFilterBuilder> configure)
        {
            return _innerBuilder.Where(configure);
        }

        IQueryBuilder IQueryBuilder.Where(IOperator filter)
        {
            return _innerBuilder.Where(filter);
        }

        IQueryBuilder IQueryBuilder.WhereAll(params IOperator[] filters)
        {
            return _innerBuilder.WhereAll(filters);
        }

        IQueryBuilder IQueryBuilder.WithFacet(string field, string name, int size)
        {
            _innerBuilder.WithFacet(field, name, size);
            return this;
        }

        IQueryBuilder IQueryBuilder.WithAliases(params string[] aliases)
        {
            _innerBuilder.WithAliases(aliases);
            return this;
        }

        public QueryObject Build()
        {
            return _innerBuilder.Build();
        }

        // Public implementations of IQueryBuilder<T> (generic interface)
        public IQueryBuilder<T> WithPagination(int page, int pageSize)
        {
            _innerBuilder.WithPagination(page, pageSize);
            return this;
        }

        public IQueryBuilder<T> WithPagination(Action<Pagination> configure)
        {
            _innerBuilder.WithPagination(configure);
            return this;
        }

        public IQueryBuilder<T> SortBy(string field, SortOrder order = SortOrder.Asc)
        {
            _innerBuilder.SortBy(field, order);
            return this;
        }

        public IQueryBuilder<T> SortBy(Expression<Func<T, object>> fieldSelector, SortOrder order = SortOrder.Asc)
        {
            var fieldName = GetFieldName(fieldSelector);

            _innerBuilder.SortBy(fieldName, order);
            return this;
        }

        public IQueryBuilder<T> Where(Action<IFilterBuilder<T>> configure)
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
                _innerBuilder.WhereAll(filters.ToArray());
            }

            return this;
        }

        public IQueryBuilder<T> WithFacet(string field, string name = null, int size = 10)
        {
            _innerBuilder.WithFacet(ToCamelCase(field), name, size);
            return this;
        }

        public IQueryBuilder<T> WithFacet(Expression<Func<T, object>> fieldSelector, string name = null, int size = 10)
        {
            if (fieldSelector == null)
            {
                throw new ArgumentNullException(nameof(fieldSelector));
            }

            var fieldName = GetFieldName(fieldSelector);

            _innerBuilder.WithFacet(fieldName, name, size);
            return this;
        }


        public IQueryBuilder<T> WithAliases(params string[] aliases)
        {
            _innerBuilder.WithAliases(aliases);
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
