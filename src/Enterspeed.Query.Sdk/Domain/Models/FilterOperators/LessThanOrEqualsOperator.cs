namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class LessThanOrEqualsOperator<TValue> : IFilterOperator<TValue>
    {
        public string Operator => "lessThanOrEquals";

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
    }
}