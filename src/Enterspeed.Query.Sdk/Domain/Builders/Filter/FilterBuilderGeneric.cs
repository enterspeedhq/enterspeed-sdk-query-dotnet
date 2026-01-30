using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Domain.Builders.Filter
{
    /// <summary>
    /// Generic implementation of IFilterBuilder with implicit entity type.
    /// Wraps the non-generic FilterBuilder and provides strongly-typed method overloads.
    /// </summary>
    /// <typeparam name="T">The entity type for this filter builder.</typeparam>
    internal class FilterBuilder<T> : IFilterBuilder<T>
    {
        private readonly FilterBuilder _innerBuilder;

        public FilterBuilder()
        {
            _innerBuilder = new FilterBuilder();
        }

        private FilterBuilder(FilterBuilder innerBuilder)
        {
            _innerBuilder = innerBuilder ?? throw new ArgumentNullException(nameof(innerBuilder));
        }

        // String-based methods (delegated to inner builder)
        public IFilterBuilder Equals<TProp>(string field, TProp value, bool? caseInsensitive = null)
        {
            return _innerBuilder.Equals(field, value, caseInsensitive);
        }

        public IFilterBuilder NotEquals<TProp>(string field, TProp value, bool? caseInsensitive = null)
        {
            return _innerBuilder.NotEquals(field, value, caseInsensitive);
        }

        public IFilterBuilder GreaterThan<TProp>(string field, TProp value)
        {
            return _innerBuilder.GreaterThan(field, value);
        }

        public IFilterBuilder GreaterThanOrEquals<TProp>(string field, TProp value)
        {
            return _innerBuilder.GreaterThanOrEquals(field, value);
        }

        public IFilterBuilder LessThan<TProp>(string field, TProp value)
        {
            return _innerBuilder.LessThan(field, value);
        }

        public IFilterBuilder LessThanOrEquals<TProp>(string field, TProp value)
        {
            return _innerBuilder.LessThanOrEquals(field, value);
        }

        public IFilterBuilder Contains<TProp>(string field, TProp value, bool? caseInsensitive = null)
        {
            return _innerBuilder.Contains(field, value, caseInsensitive);
        }

        public IFilterBuilder In<TProp>(string field, params TProp[] values)
        {
            return _innerBuilder.In(field, values);
        }

        public IFilterBuilder In<TProp>(string field, IEnumerable<TProp> values)
        {
            return _innerBuilder.In(field, values);
        }

        public IFilterBuilder In<T1, TValue>(Expression<Func<T1, TValue>> fieldSelector, IEnumerable<TValue> values)
        {
            return _innerBuilder.In(fieldSelector, values);
        }

        public IFilterBuilder Or(Action<IFilterBuilder> configure)
        {
            return _innerBuilder.Or(configure);
        }

        public IFilterBuilder And(Action<IFilterBuilder> configure)
        {
            return _innerBuilder.And(configure);
        }

        // Generic typed methods from IFilterBuilder (explicit two-type-parameter versions)¨// TODO: MAYBE WE SHOULD JUST REMOVE THIS
        IFilterBuilder IFilterBuilder.Equals<TEntity, TProp>(Expression<Func<TEntity, TProp>> fieldSelector, TProp value, bool? caseInsensitive)
        {
            return _innerBuilder.Equals(fieldSelector, value, caseInsensitive);
        }

        IFilterBuilder IFilterBuilder.NotEquals<TEntity, TProp>(Expression<Func<TEntity, TProp>> fieldSelector, TProp value, bool? caseInsensitive)
        {
            return _innerBuilder.NotEquals(fieldSelector, value, caseInsensitive);
        }

        IFilterBuilder IFilterBuilder.GreaterThan<TEntity, TProp>(Expression<Func<TEntity, TProp>> fieldSelector, TProp value)
        {
            return _innerBuilder.GreaterThan(fieldSelector, value);
        }

        IFilterBuilder IFilterBuilder.GreaterThanOrEquals<TEntity, TProp>(Expression<Func<TEntity, TProp>> fieldSelector, TProp value)
        {
            return _innerBuilder.GreaterThanOrEquals(fieldSelector, value);
        }

        IFilterBuilder IFilterBuilder.LessThan<TEntity, TProp>(Expression<Func<TEntity, TProp>> fieldSelector, TProp value)
        {
            return _innerBuilder.LessThan(fieldSelector, value);
        }

        IFilterBuilder IFilterBuilder.LessThanOrEquals<TEntity, TProp>(Expression<Func<TEntity, TProp>> fieldSelector, TProp value)
        {
            return _innerBuilder.LessThanOrEquals(fieldSelector, value);
        }

        IFilterBuilder IFilterBuilder.Contains<TEntity, TProp>(Expression<Func<TEntity, TProp>> fieldSelector, TProp value, bool? caseInsensitive)
        {
            return _innerBuilder.Contains(fieldSelector, value, caseInsensitive);
        }

        IFilterBuilder IFilterBuilder.In<TEntity, TProp>(Expression<Func<TEntity, TProp>> fieldSelector, params TProp[] values)
        {
            return _innerBuilder.In(fieldSelector, values);
        }

        // Implicit entity type methods from IFilterBuilder<T>
        public IFilterBuilder<T> Equals<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool? caseInsensitive = null)
        {
            _innerBuilder.Equals(fieldSelector, value, caseInsensitive);
            return this;
        }

        public IFilterBuilder<T> NotEquals<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool? caseInsensitive = null)
        {
            _innerBuilder.NotEquals(fieldSelector, value, caseInsensitive);
            return this;
        }

        public IFilterBuilder<T> GreaterThan<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value)
        {
            _innerBuilder.GreaterThan(fieldSelector, value);
            return this;
        }

        public IFilterBuilder<T> GreaterThanOrEquals<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value)
        {
            _innerBuilder.GreaterThanOrEquals(fieldSelector, value);
            return this;
        }

        public IFilterBuilder<T> LessThan<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value)
        {
            _innerBuilder.LessThan(fieldSelector, value);
            return this;
        }

        public IFilterBuilder<T> LessThanOrEquals<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value)
        {
            _innerBuilder.LessThanOrEquals(fieldSelector, value);
            return this;
        }

        public IFilterBuilder<T> Contains<TProp>(Expression<Func<T, TProp>> fieldSelector, TProp value, bool? caseInsensitive = null)
        {
            _innerBuilder.Contains(fieldSelector, value, caseInsensitive);
            return this;
        }

        public IFilterBuilder<T> In<TProp>(Expression<Func<T, TProp>> fieldSelector, params TProp[] values)
        {
            _innerBuilder.In(fieldSelector, values);
            return this;
        }

        public IFilterBuilder<T> In<TProp>(Expression<Func<T, TProp>> fieldSelector, IEnumerable<TProp> values)
        {
            _innerBuilder.In(fieldSelector, values);
            return this;
        }

        public IFilterBuilder<T> Or(Action<IFilterBuilder<T>> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var orBuilder = new FilterBuilder<T>();
            configure(orBuilder);

            // Use Action<IFilterBuilder> to call the inner builder's Or method
            _innerBuilder.Or(innerFilter =>
            {
                var typedBuilder = new FilterBuilder<T>(innerFilter as FilterBuilder ?? new FilterBuilder());
                configure(typedBuilder);
            });

            return this;
        }

        public IFilterBuilder<T> And(Action<IFilterBuilder<T>> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var andBuilder = new FilterBuilder<T>();
            configure(andBuilder);

            // Use Action<IFilterBuilder> to call the inner builder's And method
            _innerBuilder.And(innerFilter =>
            {
                var typedBuilder = new FilterBuilder<T>(innerFilter as FilterBuilder ?? new FilterBuilder());
                configure(typedBuilder);
            });

            return this;
        }

        internal List<IOperator> BuildFilters()
        {
            return _innerBuilder.BuildFilters();
        }
    }
}
