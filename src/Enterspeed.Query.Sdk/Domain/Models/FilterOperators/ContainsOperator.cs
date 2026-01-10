namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class ContainsOperator<TValue> : IFilterOperator<TValue>
    {
        public string Operator => "contains";

        public
            #if NET7_0_OR_GREATER
                required
            #endif 
            string Field { get; set; }
        public
            #if NET7_0_OR_GREATER
                required
            #endif
            TValue Value { get; set; }

        public bool CaseInsensitive { get; set; }
    }
}