using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Enterspeed.Query.Sdk.Domain.Builders.Filter
{
    /// <summary>
    /// Fluent interface for building filter conditions within a query.
    /// Supports nested logical grouping with AND/OR operators.
    /// Filters at the top level are implicitly ANDed together.
    /// </summary>
    public interface IFilterBuilder
    {
        /// <summary>
        /// Adds an equality filter. Supports case-insensitive comparison for compatible operators.
        /// </summary>
        /// <typeparam name="TProp">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder Equals<TProp>(string field, TProp value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds an equality filter using a strongly-typed property selector. Supports case-insensitive comparison for compatible operators.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TProp">The type of the value to compare.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder Equals<T, TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds a not-equals filter. Supports case-insensitive comparison for compatible operators.
        /// </summary>
        /// <typeparam name="TProp">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder NotEquals<TProp>(string field, TProp value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds a not-equals filter using a strongly-typed property selector. Supports case-insensitive comparison for compatible operators.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TProp">The type of the value to compare..</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder NotEquals<T, TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds a greater-than filter.
        /// </summary>
        /// <typeparam name="TProp">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder GreaterThan<TProp>(string field, TProp value);


        /// <summary>
        /// Adds a greater-than filter using a strongly-typed property selector.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TProp">The type of the value to compare..</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder GreaterThan<T, TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value);

        /// <summary>
        /// Adds a greater-than-or-equals filter.
        /// </summary>
        /// <typeparam name="TProp">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder GreaterThanOrEquals<TProp>(string field, TProp value);

        /// <summary>
        /// Adds a greater-than-or-equals filter using a strongly-typed property selector.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TProp">The type of the value to compare..</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder GreaterThanOrEquals<T, TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value);

        /// <summary>
        /// Adds a less-than filter.
        /// </summary>
        /// <typeparam name="TProp">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder LessThan<TProp>(string field, TProp value);

        /// <summary>
        /// Adds a less-than filter using a strongly-typed property selector.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TProp">The type of the value to compare..</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder LessThan<T, TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value);

        /// <summary>
        /// Adds a less-than-or-equals filter.
        /// </summary>
        /// <typeparam name="TProp">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder LessThanOrEquals<TProp>(string field, TProp value);

        /// <summary>
        /// Adds a less-than-or-equals filter using a strongly-typed property selector.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TProp">The type of the value to compare.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder LessThanOrEquals<T, TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value);

        /// <summary>
        /// Adds a contains filter for string matching. Supports case-insensitive comparison.
        /// </summary>
        /// <typeparam name="TProp">The type of the value to search for.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The property to search for (supports wildcards).</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder Contains<TProp>(string field, TProp value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds a contains filter for string matching using a strongly-typed property selector. Supports case-insensitive comparison.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TProp">The type of the values to match against.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="value">The value to search for (supports wildcards).</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder Contains<T, TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds an IN filter (value must match one of the provided values).
        /// </summary>
        /// <typeparam name="TProp">The type of the values to match against.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="values">The collection of values to match against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder In<TProp>(string field, params TProp[] values);

        /// <summary>
        /// Adds an IN filter (value must match one of the provided values) using a strongly-typed property selector.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TProp">The type of the values to match against.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="values">The collection of values to match against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder In<T, TProp>(Expression<Func<T, TProp>> fieldSelector, params TProp[] values);

        /// <summary>
        /// Adds an IN filter (value must match one of the provided values) using a strongly-typed property selector.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TValue">The type of the values to match against.</typeparam>
        /// <param name="fieldSelector">Expression to select the property to filter on.</param>
        /// <param name="values">The collection of values to match against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder In<T, TValue>(Expression<Func<T, TValue>> fieldSelector, IEnumerable<TValue> values);


        /// <summary>
        /// Creates a nested OR group. Multiple conditions inside the lambda are implicitly ORed.
        /// </summary>
        /// <param name="configure">Lambda expression to configure the OR group filters.</param>
        /// <returns>The filter builder for method chaining.</returns>
        /// <example>
        /// .Or(o => o
        ///     .Equals("status", "active")
        ///     .Equals("status", "pending"))
        /// </example>
        IFilterBuilder Or(Action<IFilterBuilder> configure);

        /// <summary>
        /// Creates a nested AND group. Useful for complex precedence control.
        /// Multiple conditions inside the lambda are implicitly ANDed.
        /// </summary>
        /// <param name="configure">Lambda expression to configure the AND group filters.</param>
        /// <returns>The filter builder for method chaining.</returns>
        /// <example>
        /// .And(a => a
        ///     .GreaterThan("price", 100)
        ///     .LessThan("price", 200))
        /// </example>
        IFilterBuilder And(Action<IFilterBuilder> configure);
    }
}
