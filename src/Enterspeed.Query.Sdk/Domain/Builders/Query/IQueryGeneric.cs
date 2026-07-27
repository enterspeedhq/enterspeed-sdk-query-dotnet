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
    public interface IQuery<T> : IQuery
    {
        /// <summary>
        /// Sets the pagination for the query.
        /// </summary>
        /// <param name="page">The page number (zero-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQuery<T> WithPagination(int page, int pageSize);

        /// <summary>
        /// Sets the pagination for the query using a configuration delegate.
        /// </summary>
        /// <param name="configure">Action to configure pagination settings.</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQuery<T> WithPagination(Action<Pagination> configure);

        /// <summary>
        /// Adds a sort criterion using a string field name.
        /// </summary>
        /// <param name="field">The field to sort by.</param>
        /// <param name="order">The sort order (Asc or Desc).</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQuery<T> SortBy(string field, SortOrder order = SortOrder.Asc);

        /// <summary>
        /// Adds a sort criterion using a property selector with implicit entity type.
        /// </summary>
        /// <param name="fieldSelector">Expression to select the property. The resulting field name is always lower case unless a JsonPropertyName attribute is present, which overrides the default naming.</param>
        /// <param name="order">The sort order (Asc or Desc).</param>
        /// <returns>The query builder for method chaining.</returns>
        IQuery<T> SortBy(Expression<Func<T, object>> fieldSelector, SortOrder order = SortOrder.Asc);

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
        IQuery<T> Where(Action<IFilterBuilder<T>> configure);

        /// <summary>
        /// Adds a facet aggregation using a string field name.
        /// </summary>
        /// <param name="field">The field to facet on and has force lowercasing.</param>
        /// <param name="name">Optional name for the facet (defaults to field name).</param>
        /// <param name="size">Maximum number of facet values to return.</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQuery<T> WithFacet(string field, string name = null, int size = 10);

        /// <summary>
        /// Adds a facet aggregation using a property selector with implicit entity type.
        /// </summary>
        /// <param name="fieldSelector">Expression to select the property. The resulting field name is always lower case unless a JsonPropertyName attribute is present, which overrides the default naming.</param>
        /// <param name="name">Optional name for the facet (defaults to field name).</param>
        /// <param name="size">Maximum number of facet values to return.</param>
        /// <returns>The query builder for method chaining.</returns>
        IQuery<T> WithFacet(Expression<Func<T, object>> fieldSelector, string name = null, int size = 10);

        /// <summary>
        /// Adds view aliases to retrieve different presentations of the same content.
        /// </summary>
        /// <param name="aliases">The view aliases to include.</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQuery<T> WithAliases(params string[] aliases);

        /// <summary>
        /// Applies a relevance search to a text field. Required in order to sort by "_score".
        /// Calling this more than once replaces the previous search.
        /// </summary>
        /// <param name="field">The text field to search.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="literal">When true, matches indexed tokens exactly (no fuzziness).</param>
        /// <returns>The query builder for method chaining.</returns>
        new IQuery<T> WithSearch(string field, string value, bool literal = false);

        /// <summary>
        /// Applies a relevance search using a property selector with implicit entity type.
        /// </summary>
        /// <param name="fieldSelector">Expression to select the property. The resulting field name is always lower case unless a JsonPropertyName attribute is present, which overrides the default naming.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="literal">When true, matches indexed tokens exactly (no fuzziness).</param>
        /// <returns>The query builder for method chaining.</returns>
        IQuery<T> WithSearch(Expression<Func<T, object>> fieldSelector, string value, bool literal = false);
    }
}
