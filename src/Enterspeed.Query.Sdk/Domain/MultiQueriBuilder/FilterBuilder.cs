using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterspeed.Query.Sdk.Domain.Models;
using Enterspeed.Query.Sdk.Domain.Models.FilterOperators;
using Enterspeed.Query.Sdk.Domain.Models.LogicalOperators;

namespace Enterspeed.Query.Sdk.Domain.MultiQueriBuilder
{
    /// <summary>
    /// Internal implementation of IFilterBuilder for accumulating filter operators.
    /// Builds a tree of filter operators that matches the Enterspeed Query API structure.
    /// </summary>
    internal class FilterBuilder : IFilterBuilder
    {
        private readonly List<IOperator> _filters = new List<IOperator>();

        public IFilterBuilder Equals<TValue>(string field, TValue value, bool? caseInsensitive = null)
        {
            var op = new EqualsOperator<TValue> { Field = field, Value = value };
            SetCaseInsensitiveIfSupported(op, caseInsensitive);
            _filters.Add(op);
            return this;
        }

        public IFilterBuilder NotEquals<TValue>(string field, TValue value, bool? caseInsensitive = null)
        {
            var op = new NotEqualsOperator<TValue> { Field = field, Value = value };
            SetCaseInsensitiveIfSupported(op, caseInsensitive);
            _filters.Add(op);
            return this;
        }

        public IFilterBuilder GreaterThan<TValue>(string field, TValue value)
        {
            _filters.Add(new GreaterThanOperator<TValue> { Field = field, Value = value });
            return this;
        }

        public IFilterBuilder GreaterThanOrEquals<TValue>(string field, TValue value)
        {
            _filters.Add(new GreaterThanOrEqualsOperator<TValue> { Field = field, Value = value });
            return this;
        }

        public IFilterBuilder LessThan<TValue>(string field, TValue value)
        {
            _filters.Add(new LessThanOperator<TValue> { Field = field, Value = value });
            return this;
        }

        public IFilterBuilder LessThanOrEquals<TValue>(string field, TValue value)
        {
            _filters.Add(new LessThanOrEqualsOperator<TValue> { Field = field, Value = value });
            return this;
        }

        public IFilterBuilder Contains<TValue>(string field, TValue value, bool? caseInsensitive = null)
        {
            var op = new ContainsOperator<TValue> { Field = field, Value = value };
            SetCaseInsensitiveIfSupported(op, caseInsensitive);
            _filters.Add(op);
            return this;
        }

        public IFilterBuilder In<TValue>(string field, params TValue[] values)
        {
            _filters.Add(new InOperator<TValue> { Field = field, Value = values.ToList() });
            return this;
        }

        public IFilterBuilder And()
        {
            // Cosmetic only - filters are implicitly ANDed at the top level
            return this;
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

            if (orFilters.Count == 0) return this;

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
            if (_filters.Count == 0)
            {
                return new List<IOperator>();
            }

            if (_filters.Count == 1)
            {
                return _filters;
            }

            // Multiple filters: wrap in implicit AND
            return new List<IOperator> { new AndOperator { And = _filters } };
        }

        /// <summary>
        /// Sets the CaseInsensitive property on an operator if it supports it.
        /// Uses reflection to detect and set the property dynamically.
        /// </summary>
        private void SetCaseInsensitiveIfSupported(FilterOperator op, bool? caseInsensitive)
        {
            if (!caseInsensitive.HasValue)
            {
                return;
            }

            var property = op.GetType().GetProperty("CaseInsensitive", BindingFlags.Public | BindingFlags.Instance);
            if (property != null && property.PropertyType == typeof(bool) && property.CanWrite)
            {
                property.SetValue(op, caseInsensitive.Value);
            }
        }
    }
}
