using System;

namespace Enterspeed.Query.Sdk.Domain.MultiQueriBuilder
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
        /// <typeparam name="TValue">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder Equals<TValue>(string field, TValue value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds a not-equals filter. Supports case-insensitive comparison for compatible operators.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder NotEquals<TValue>(string field, TValue value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds a greater-than filter.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder GreaterThan<TValue>(string field, TValue value);

        /// <summary>
        /// Adds a greater-than-or-equals filter.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder GreaterThanOrEquals<TValue>(string field, TValue value);

        /// <summary>
        /// Adds a less-than filter.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder LessThan<TValue>(string field, TValue value);

        /// <summary>
        /// Adds a less-than-or-equals filter.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to compare.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder LessThanOrEquals<TValue>(string field, TValue value);

        /// <summary>
        /// Adds a contains filter for string matching. Supports case-insensitive comparison.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to search for.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="value">The value to search for (supports wildcards).</param>
        /// <param name="caseInsensitive">Optional. If true and operator supports it, performs case-insensitive comparison.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder Contains<TValue>(string field, TValue value, bool? caseInsensitive = null);

        /// <summary>
        /// Adds an IN filter (value must match one of the provided values).
        /// </summary>
        /// <typeparam name="TValue">The type of the values to match against.</typeparam>
        /// <param name="field">The field name to filter on.</param>
        /// <param name="values">The collection of values to match against.</param>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder In<TValue>(string field, params TValue[] values);

        /// <summary>
        /// Cosmetic separator for readability. Has no functional effect.
        /// Filters are implicitly ANDed at the top level.
        /// Use .And(Action) for explicit nested AND groups.
        /// </summary>
        /// <returns>The filter builder for method chaining.</returns>
        IFilterBuilder And();

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