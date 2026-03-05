using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Domain.SystemTextJson
{
    public class SortOrderJsonConverter : JsonConverter<SortOrder>
    {
        public override SortOrder Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            foreach (SortOrder order in Enum.GetValues(typeof(SortOrder)))
            {
                var attr = typeof(SortOrder).GetField(order.ToString())?.GetCustomAttribute<JsonPropertyNameAttribute>();
                if (attr != null && attr.Name == value)
                {
                    return order;
                }
            }
            throw new JsonException($"Unknown sort order: {value}");
        }

        public override void Write(Utf8JsonWriter writer, SortOrder value, JsonSerializerOptions options)
        {
            var attr = typeof(SortOrder).GetField(value.ToString())?.GetCustomAttribute<JsonPropertyNameAttribute>();
            writer.WriteStringValue(attr?.Name ?? value.ToString());
        }
    }
}
