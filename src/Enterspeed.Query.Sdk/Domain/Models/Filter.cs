using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.Models
{
    public interface IOperator
    {
    }

    public abstract class LogicalOperator : IOperator
    {
    }

    public class AndOperator : LogicalOperator
    {
        [JsonPropertyName("and")]
        public List<IOperator> And { get; set; } = new List<IOperator>();
    }

    public class OrOperator : LogicalOperator
    {
        [JsonPropertyName("or")]
        public List<IOperator> Or { get; set; } = new List<IOperator>();
    }



    public abstract class FilterOperator : IOperator
    {
        public string Field { get; set; }
        public abstract string Operator { get; }
    }

    public abstract class FilterOperator<TValue> : FilterOperator
    {
        public abstract TValue Value { get; set; }
    }

    public class EqualsOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "equals";
        public override TValue Value { get; set; }
        public bool CaseInsensitive { get; set; }
    }

    public class NotEqualsOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "notEquals";
        public override TValue Value { get; set; }
        public bool CaseInsensitive { get; set; }
    }

    public class ContainsOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "contains";
        public override TValue Value { get; set; } // TODO can we help with wildcards if it's a string?
        public bool CaseInsensitive { get; set; }
    }

    public class GreaterThanOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "greaterThan";
        public override TValue Value { get; set; }
    }

    public class LessThanOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "lessThan";
        public override TValue Value { get; set; }
    }

    public class LessThanOrEqualsOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "lessThanOrEquals";
        public override TValue Value { get; set; }
    }

    public class InOperator<TValue> : FilterOperator<List<TValue>>
    {
        public override string Operator => "in";
        public override List<TValue> Value { get; set; }
    }
}