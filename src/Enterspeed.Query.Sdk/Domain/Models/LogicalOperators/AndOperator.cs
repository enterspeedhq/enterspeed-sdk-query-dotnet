using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.Models.LogicalOperators
{
    public class AndOperator : LogicalOperator
    {
        [JsonPropertyName("and")]
        public List<IOperator> And { get; set; } = new List<IOperator>();
    }
}