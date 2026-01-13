namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class GreaterThanOrEqualsOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "greaterThanOrEquals";
        public override TValue Value { get; set; }
    }
}