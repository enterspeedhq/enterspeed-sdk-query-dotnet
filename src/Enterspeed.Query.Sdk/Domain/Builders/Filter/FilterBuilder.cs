using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
using Enterspeed.Query.Sdk.Domain.Models.LogicalOperators;
using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.Builders.Filter
{
    /// <summary>
    /// Internal implementation of IFilterBuilder for accumulating filter operators.
    /// Builds a tree of filter operators that matches the Enterspeed Query API structure.
    /// </summary>
    internal class FilterBuilder : IFilterBuilder
    {
        private readonly List<IOperator> _filters = new List<IOperator>();


        public IFilterBuilder Equals<TValue>(string field, TValue value, bool caseInsensitive = false)
        {
            var op = new EqualsOperator<TValue> { Field = field, Value = value, CaseInsensitive = caseInsensitive};
            _filters.Add(op);
            return this;
        }

        public IFilterBuilder Equals<T, TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool caseInsensitive = false)
        {
            var fieldName = ExtractFieldName(fieldSelector);
            return Equals(fieldName, value, caseInsensitive);
        }

        public IFilterBuilder NotEquals<TValue>(string field, TValue value, bool caseInsensitive = false)
        {
            var op = new NotEqualsOperator<TValue> { Field = field, Value = value, CaseInsensitive = caseInsensitive};
            _filters.Add(op);
            return this;
        }

        public IFilterBuilder NotEquals<T, TValue>(Expression<Func<T, TValue>> fieldSelector, TValue value, bool caseInsensitive = false)
        {
            var fieldName = ExtractFieldName(fieldSelector);
            return NotEquals(fieldName, value, caseInsensitive);
        }

        public IFilterBuilder GreaterThan<TValue>(string field, TValue value)
        {
            _filters.Add(new GreaterThanOperator<TValue> { Field = field, Value = value });
            return this;
        }

        public IFilterBuilder GreaterThan<T, TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value)
        {
            var fieldName = ExtractFieldName(fieldSelector);
            return GreaterThan(fieldName, value);
        }

        public IFilterBuilder GreaterThanOrEquals<TValue>(string field, TValue value)
        {
            _filters.Add(new GreaterThanOrEqualsOperator<TValue> { Field = field, Value = value });
            return this;
        }

        public IFilterBuilder GreaterThanOrEquals<T, TValue>(Expression<Func<T, TValue>> fieldSelector, TValue value)
        {
            var fieldName = ExtractFieldName(fieldSelector);
            return GreaterThanOrEquals(fieldName, value);
        }

        public IFilterBuilder LessThan<TValue>(string field, TValue value)
        {
            _filters.Add(new LessThanOperator<TValue> { Field = field, Value = value });
            return this;
        }

        public IFilterBuilder LessThan<T, TValue>(Expression<Func<T, TValue>> fieldSelector, TValue value)
        {
            var fieldName = ExtractFieldName(fieldSelector);
            return LessThan(fieldName, value);
        }

        public IFilterBuilder LessThanOrEquals<TValue>(string field, TValue value)
        {
            _filters.Add(new LessThanOrEqualsOperator<TValue> { Field = field, Value = value });
            return this;
        }

        public IFilterBuilder LessThanOrEquals<T, TValue>(Expression<Func<T, TValue>> fieldSelector, TValue value)
        {
            var fieldName = ExtractFieldName(fieldSelector);
            return LessThanOrEquals(fieldName, value);
        }

        public IFilterBuilder Contains<TValue>(string field, TValue value, bool caseInsensitive = false)
        {
            var op = new ContainsOperator<TValue> { Field = field, Value = value, CaseInsensitive = caseInsensitive};
            _filters.Add(op);
            return this;
        }

        public IFilterBuilder Contains<T, TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool caseInsensitive = false)
        {
            var fieldName = ExtractFieldName(fieldSelector);
            return Contains(fieldName, value, caseInsensitive);
        }

        public IFilterBuilder In<TValue>(string field, params TValue[] values)
        {
            _filters.Add(new InOperator<TValue> { Field = field, Value = values.ToList() });
            return this;
        }

        public IFilterBuilder In<TValue>(string field, IEnumerable<TValue> values)
        {
            _filters.Add(new InOperator<TValue> { Field = field, Value = values?.ToList() ?? new List<TValue>() });
            return this;
        }

        public IFilterBuilder In<T, TValue>(Expression<Func<T, TValue>> fieldSelector, params TValue[] values)
        {
            var fieldName = ExtractFieldName(fieldSelector);
            return In(fieldName, values);
        }

        public IFilterBuilder In<T, TValue>(Expression<Func<T, TValue>> fieldSelector, IEnumerable<TValue> values)
        {
            var fieldName = ExtractFieldName(fieldSelector);
            return In(fieldName, values);
        }

        public IFilterBuilder Or(Action<IFilterBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var orBuilder = new FilterBuilder();
            configure(orBuilder);

            var orFilters = orBuilder.BuildFilters();

            if (orFilters.Count == 0)
            {
                return this;
            }

            var orOperator = new OrOperator
            {
                Or = orFilters
            };
            _filters.Add(orOperator);

            return this;
        }

        public IFilterBuilder And(Action<IFilterBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var andBuilder = new FilterBuilder();
            configure(andBuilder);

            var andFilters = andBuilder.BuildFilters();
            if (andFilters.Count > 0)
            {
                var andOperator = new AndOperator
                {
                    And = andFilters
                };
                _filters.Add(andOperator);
            }

            return this;
        }


        /// <summary>
        /// Builds the final filter operator structure.
        /// Returns the accumulated filters, wrapping in AndOperator if multiple.
        /// </summary>
        internal List<IOperator> BuildFilters()
        {
            switch (_filters.Count)
            {
                case 0:
                    return new List<IOperator>();
                case 1:
                    return _filters;
                default:
                    return new List<IOperator>
                    {
                        new AndOperator
                        {
                            And = _filters
                        }
                    };
            }
        }

        /// <summary>
        /// Extracts the field name from a property selector expression.
        /// If the property has a JsonPropertyName attribute, that name is used (case preserved).
        /// Otherwise, the property name is converted to camelCase to match the JSON serialization convention used by the Query API.
        /// </summary>
        private static string ExtractFieldName<T, TProp>(Expression<Func<T, TProp>> fieldSelector)
        {
            if (fieldSelector == null)
            {
                throw new ArgumentNullException(nameof(fieldSelector));
            }

            // Handle boxing/unboxing conversions (e.g., value types to object)
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
            if (propertyInfo != null)
            {
                var jsonPropertyNameAttr = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (jsonPropertyNameAttr != null && !string.IsNullOrWhiteSpace(jsonPropertyNameAttr.Name))
                {
                    return jsonPropertyNameAttr.Name;
                }
            }

            var propertyName = member.Member.Name;

            // Convert to camelCase to match JSON serialization
            return ToCamelCase(propertyName);
        }

        /// <summary>
        /// Converts a PascalCase or camelCase string to camelCase.
        /// This matches the JsonNamingPolicy.CamelCase behavior used in the SDK.
        /// </summary>
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
