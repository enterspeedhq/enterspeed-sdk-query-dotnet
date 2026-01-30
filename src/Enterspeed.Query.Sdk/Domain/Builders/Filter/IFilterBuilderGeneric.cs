using System;
using System.Linq.Expressions;

namespace Enterspeed.Query.Sdk.Domain.Builders.Filter
{
    /// <summary>
    /// Generic fluent interface for building filter conditions with implicit entity type.
    /// The entity type T is inferred from the query context, eliminating the need to specify it on each filter method.
    /// Supports nested logical grouping with AND/OR operators.
    /// Filters at the top level are implicitly ANDed together.
    /// </summary>
    /// <typeparam name="T">The entity type for this filter builder.</typeparam>
    public interface IFilterBuilder<T> : IFilterBuilder
    {
        /// <summary>
        /// Adds an equality filter using a property selector with implicit entity type.
        /// </summary>
        /// <typeparam name="TProp">The property type to compare.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder<T> Equals<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds a not-equals filter using a property selector with implicit entity type.
        /// </summary>
        /// <typeparam name="TProp">The property type to compare.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder<T> NotEquals<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds a greater-than filter using a property selector with implicit entity type.
        /// </summary>
        /// <typeparam name="TProp">The property type to compare.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder<T> GreaterThan<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value);

        /// <summary>
        /// Adds a greater-than-or-equals filter using a property selector with implicit entity type.
        /// </summary>
        /// <typeparam name="TProp">The property type to compare.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder<T> GreaterThanOrEquals<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value);

        /// <summary>
        /// Adds a less-than filter using a property selector with implicit entity type.
        /// </summary>
        /// <typeparam name="TProp">The property type to compare.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder<T> LessThan<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value);

        /// <summary>
        /// Adds a less-than-or-equals filter using a property selector with implicit entity type.
        /// </summary>
        /// <typeparam name="TProp">The property type to compare.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder<T> LessThanOrEquals<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value);

        /// <summary>
        /// Adds a contains filter using a property selector with implicit entity type.
        /// </summary>
        /// <typeparam name="TProp">The property type to search.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to search for (supports wildcards).</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder<T> Contains<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds an IN filter using a property selector with implicit entity type.
        /// </summary>
        /// <typeparam name="TProp">The property type to match.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="values">The collection of values to match against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder<T> In<TProp>(Expression<Func<T, TProp>> fieldSelector, params TProp[] values);

        /// <summary>
        /// Creates a nested OR group with implicit entity type.
        /// </summary>
        /// <param name="configure">Lambda expression to configure the OR group filters.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder<T> Or(Action<IFilterBuilder<T>> configure);

        /// <summary>
        /// Creates a nested AND group with implicit entity type.
        /// </summary>
        /// <param name="configure">Lambda expression to configure the AND group filters.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder<T> And(Action<IFilterBuilder<T>> configure);
    }
}
