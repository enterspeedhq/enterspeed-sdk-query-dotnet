using System.Text.Json.Serialization;

namespace Enterspeed.Query.Sdk.Domain.Models
{
    public enum SortOrder
    {
        [JsonPropertyName("desc")]
        Desc,

        [JsonPropertyName("asc")]
        Asc
    }
}