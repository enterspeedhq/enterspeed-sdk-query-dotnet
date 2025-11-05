namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class ContainsOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "contains";
        public override TValue Value { get; set; }
        public bool CaseInsensitive { get; set; }
    }
}