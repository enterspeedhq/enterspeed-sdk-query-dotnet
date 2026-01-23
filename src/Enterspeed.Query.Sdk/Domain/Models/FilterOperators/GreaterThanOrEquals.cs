namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class GreaterThanOrEquals<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "greaterThanOrEquals";
        public override TValue Value { get; set; }
    }
}
