using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.Models
{
    public class Sort
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("order")]
        public SortOrder Order { get; set; }
    }
}