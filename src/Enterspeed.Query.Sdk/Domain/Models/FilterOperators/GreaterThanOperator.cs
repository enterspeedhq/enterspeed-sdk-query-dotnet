namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class GreaterThanOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "greaterThan";
        public override TValue Value { get; set; }
    }
}