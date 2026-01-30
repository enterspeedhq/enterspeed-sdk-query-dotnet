using System;
using System.Linq.Expressions;
using Enterspeed.Query.Sdk.Domain.Builders.Filter;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Domain.Builders.Query
{
    /// <summary>
    /// Generic fluent interface for building queries with implicit entity type.
    /// The entity type T is inferred from the AddQuery context, eliminating the need to specify it on filter and sort methods.
    /// </summary>
    /// <typeparam name="T">The entity type for this query builder.</typeparam>
    public interface IQueryBuilder<T> : IQueryBuilder
    {
        /// <summary>
        /// Sets the pagination for the query.
        /// </summary>
        /// <param name="page">The page number (zero-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQueryBuilder<T> WithPagination(int page, int pageSize);

        /// <summary>
        /// Sets the pagination for the query using a configuration delegate.
        /// </summary>
        /// <param name="configure">Action to configure pagination settings.</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQueryBuilder<T> WithPagination(Action<Pagination> configure);

        new IQueryBuilder<T> WithPagination(Pagination pagination);

        /// <summary>
        /// Adds a sort criterion using a string field name.
        /// </summary>
        /// <param name="field">The field to sort by.</param>
        /// <param name="order">The sort order (Asc or Desc).</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQueryBuilder<T> SortBy(string field, SortOrder order = SortOrder.Asc);

        /// <summary>
        /// Adds a sort criterion using a property selector with implicit entity type.
        /// </summary>
        /// <param name="fieldSelector">Expression to select the property to sort by.</param>
        /// <param name="order">The sort order (Asc or Desc).</param>
        /// <returns>The query builder for method chaining.</returns>
        IQueryBuilder<T> SortBy(Expression<Func<T, object>> fieldSelector, SortOrder order = SortOrder.Asc);

        /// <summary>
        /// Adds filters using a generic filter builder with implicit entity type.
        /// Multiple Where() calls accumulate with implicit AND logic.
        /// </summary>
        /// <param name="configure">Lambda expression to configure filter conditions using IFilterBuilder&lt;T&gt;.</param>
        /// <returns>The query builder for method chaining.</returns>
        /// <example>
        /// .Where(filter => filter
        ///     .Equals(p => p.Active, true)
        ///     .GreaterThan(p => p.Price, 50m))
        /// </example>
        IQueryBuilder<T> Where(Action<IFilterBuilder<T>> configure);

        /// <summary>
        /// Adds a facet aggregation using a string field name.
        /// </summary>
        /// <param name="field">The field to facet on.</param>
        /// <param name="name">Optional name for the facet (defaults to field name).</param>
        /// <param name="size">Maximum number of facet values to return.</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQueryBuilder<T> WithFacet(string field, string name = null, int size = 10);

        /// <summary>
        /// Adds a facet aggregation using a property selector with implicit entity type.
        /// </summary>
        /// <param name="fieldSelector">Expression to select the property to facet on.</param>
        /// <param name="name">Optional name for the facet (defaults to field name).</param>
        /// <param name="size">Maximum number of facet values to return.</param>
        /// <returns>The query builder for method chaining.</returns>
        IQueryBuilder<T> WithFacet(Expression<Func<T, object>> fieldSelector, string name = null, int size = 10);

        /// <summary>
        /// Adds view aliases to retrieve different presentations of the same content.
        /// </summary>
        /// <param name="aliases">The view aliases to include.</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQueryBuilder<T> WithAliases(params string[] aliases);
    }
}
