namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class EqualsOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "equals";
        public override TValue Value { get; set; }
        public bool CaseInsensitive { get; set; }
    }
}