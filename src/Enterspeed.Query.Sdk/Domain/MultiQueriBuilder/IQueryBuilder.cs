using System;
using System.Collections.Generic;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;

namespace Enterspeed.Query.Sdk.Domain.MultiQueriBuilder
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
        /// Adds a sort criterion to the query.
        /// </summary>
        /// <param name="field">The field to sort by.</param>
        /// <param name="order">The sort order (Asc or Desc).</param>
        IQueryBuilder SortBy(string field, SortOrder order = SortOrder.Asc);

        /// <summary>
        /// Adds a filter to the query.
        /// </summary>
        /// <param name="filter">The filter operator to apply.</param>
        IQueryBuilder Where(FilterOperator filter);

        /// <summary>
        /// Adds multiple filters with AND logic to the query.
        /// </summary>
        /// <param name="filters">Collection of filter operators to apply.</param>
        IQueryBuilder WhereAll(params FilterOperator[] filters);

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
