using System;
using System.Linq.Expressions;
using Enterspeed.Query.Sdk.Domain.Builders.Filter;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Domain.Builders.Query
{
    /// <summary>
    /// Fluent interface for building individual queries within a multi-query request.
    /// </summary>
    public interface IQueryBuilder
    {
        /// <summary>
        /// Sets the pagination for the query.
        /// </summary>
        /// <param name="page">The page number (zero-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        IQueryBuilder WithPagination(int page, int pageSize);

        /// <summary>
        /// Sets the pagination for the query using a configuration delegate.
        /// </summary>
        /// <param name="configure">Action to configure pagination settings.</param>
        /// <returns>The query builder for method chaining.</returns>
        IQueryBuilder WithPagination(Action<Pagination> configure);

        /// <summary>
        /// Sets the pagination for the query using a Pagination Class.
        /// </summary>
        /// <param name="pagination">A pagination class.</param>
        /// <returns>The query builder for method chaining.</returns>
        IQueryBuilder WithPagination(Pagination pagination);

        /// <summary>
        /// Adds a sort criterion to the query.
        /// </summary>
        /// <param name="field">The field to sort by.</param>
        /// <param name="order">The sort order (Asc or Desc).</param>
        IQueryBuilder SortBy(string field, SortOrder order = SortOrder.Asc);

        /// <summary>
        /// Adds a sort criterion to the query using a strongly-typed property selector.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to sort by.</param>
        /// <param name="order">The sort order (Asc or Desc).</param>
        IQueryBuilder SortBy<T>(Expression<Func<T, object>> fieldSelector, SortOrder order = SortOrder.Asc);


        /// <summary>
        /// Adds filters using a fluent lambda-based builder pattern.
        /// Multiple Where() calls accumulate with implicit AND logic.
        /// Filters within a single Where() call are also implicitly ANDed.
        /// </summary>
        /// <param name="configure">Lambda expression to configure filter conditions.</param>
        /// <returns>The query builder for method chaining.</returns>
        /// <example>
        /// .Where(f => f
        ///     .Equals("status", "active")
        ///     .GreaterThan(age", 18))
        /// </example>
        IQueryBuilder Where(Action<IFilterBuilder> configure);

        /// <summary>
        /// Adds a filter to the query using a pre-constructed FilterOperator.
        /// This is the power-user API for complex scenarios.
        /// </summary>
        /// <param name="filter">The filter operator to apply.</param>
        IQueryBuilder Where(IOperator filter);

        /// <summary>
        /// Adds multiple filters with AND logic to the query.
        /// This is the power-user API for complex scenarios.
        /// </summary>
        /// <param name="filters">Collection of filter operators to apply.</param>
        IQueryBuilder WhereAll(params IOperator[] filters);

        /// <summary>
        /// Adds a facet aggregation to the query.
        /// </summary>
        /// <param name="field">The field to facet on.</param>
        /// <param name="name">Optional name for the facet (defaults to field name).</param>
        /// <param name="size">Maximum number of facet values to return.</param>
        IQueryBuilder WithFacet(string field, string name = null, int size = 10);

        /// <summary>
        /// Adds view aliases to retrieve different presentations of the same content.
        /// </summary>
        /// <param name="aliases">The view aliases to include.</param>
        IQueryBuilder WithAliases(params string[] aliases);

        /// <summary>
        /// Builds the query object with the configured parameters.
        /// </summary>
        /// <returns>The constructed QueryObject.</returns>
        QueryObject Build();
    }
}
