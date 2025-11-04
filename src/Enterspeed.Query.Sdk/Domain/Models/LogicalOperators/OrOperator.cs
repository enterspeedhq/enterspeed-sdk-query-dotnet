using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.Models.LogicalOperators
{
    public class OrOperator : LogicalOperator
    {
        [JsonPropertyName("or")]
        public List<IOperator> Or { get; set; } = new List<IOperator>();
    }
}