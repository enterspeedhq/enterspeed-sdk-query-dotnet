namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class NotEqualsOperator<TValue> : FilterOperator<TValue>
    {
        public override string Operator => "notEquals";
        public override TValue Value { get; set; }
        public bool CaseInsensitive { get; set; }
    }
}