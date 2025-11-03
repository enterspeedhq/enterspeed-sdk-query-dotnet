using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.Models
{
    public class MultiQueryObject : QueryObject
    {
        [JsonPropertyName("index")]
        public string Index { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}