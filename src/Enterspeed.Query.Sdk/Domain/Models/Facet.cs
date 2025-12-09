using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.Models
{
    public class Facet
    {
        [JsonPropertyName("field")]
        public
            #if NET7_0_OR_GREATER
                required
            #endif 
            string Field { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("size")]
        public int Size { get; set; }
    }
}