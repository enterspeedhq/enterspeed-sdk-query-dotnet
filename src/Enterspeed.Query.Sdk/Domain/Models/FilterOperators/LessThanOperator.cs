namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class LessThanOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "lessThan";
        public override TValue Value { get; set; }
    }
}