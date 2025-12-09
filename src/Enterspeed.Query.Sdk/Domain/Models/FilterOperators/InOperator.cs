using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class InOperator<TValue> : IFilterOperator<List<TValue>>
    {
        public string Operator => "in";

        public 
            #if NET7_0_OR_GREATER
                required
            #endif
            string Field { get; set; }

        public
            #if NET7_0_OR_GREATER
                required
            #endif
            List<TValue> Value { get; set; }
    }
}