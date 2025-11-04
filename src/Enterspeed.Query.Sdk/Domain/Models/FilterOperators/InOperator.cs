using System.Collections.Generic;

namespace Enterspeed.Query.Sdk.Domain.Models.FilterOperators
{
    public class InOperator<TValue> : FilterOperator<List<TValue>>
    {
        public override string Operator => "in";
        public override List<TValue> Value { get; set; }
    }
}