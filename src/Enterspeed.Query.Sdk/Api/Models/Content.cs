using System.Text.Json;

namespace Enterspeed.Query.Sdk.Api.Models
{
    public class Content : IContent
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public Content(JsonElement value, JsonSerializerOptions serializerOptions)
        {
            _serializerOptions = serializerOptions;
            Value = value;
        }

        public JsonElement Value { get; set; }
        public T GetContent<T>() => Value.Deserialize<T>(_serializerOptions);

        public T GetContent<T>(string propertyName) =>
            Value.TryGetProperty(propertyName, out var value)
                ? value.Deserialize<T>(_serializerOptions)
                : default;
    }
}
