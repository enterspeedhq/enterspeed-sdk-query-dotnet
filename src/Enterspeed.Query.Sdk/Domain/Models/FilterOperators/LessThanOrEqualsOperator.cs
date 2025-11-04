namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class LessThanOrEqualsOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "lessThanOrEquals";
        public override TValue Value { get; set; }
    }
}