using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query;

namespace Enterspeed.Query.Sdk.Domain.SystemTextJson
{
    /// <summary>
    /// Custom converter for polymorphic QueryResponse
    /// </summary>
    public class QueryResponseConverter : JsonConverter<QueryResponse>
    {
        private const string DiscriminatorPropertyName = "status";

        public override QueryResponse Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            var jsonDoc = JsonDocument.ParseValue(ref reader);
            try
            {
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty(DiscriminatorPropertyName, out JsonElement discriminatorElement))
                {
                    throw new JsonException($"Missing required discriminator property '{DiscriminatorPropertyName}'");
                }

                var discriminatorValue = GetDiscriminatorValue(discriminatorElement);
                var targetType = GetTargetType(discriminatorValue);

                var optionsWithoutConverter = CreateOptionsWithoutThisConverter(options);

                var concreteResponse = JsonSerializer.Deserialize(
                    root.GetRawText(),
                    targetType,
                    optionsWithoutConverter);

                return (QueryResponse)concreteResponse;
            }
            finally
            {
                jsonDoc.Dispose();
            }
        }

        public override void Write(Utf8JsonWriter writer, QueryResponse value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, options);
        }

        private Type GetTargetType(object discriminatorValue)
        {
            var normalizedValue = NormalizeDiscriminator(discriminatorValue);

            switch (normalizedValue)
            {
                case 0:
                    return typeof(QueryResponseSuccess<Dictionary<string, object>>);
                case 1:
                    return typeof(QueryResponseError);
                default:
                    throw new JsonException($"Unknown discriminator value: {discriminatorValue}");
            }
        }

        private int NormalizeDiscriminator(object discriminatorValue)
        {
            switch (discriminatorValue)
            {
                case int value:
                    return value;
                case long value:
                    return (int)value;
                case string value:
                    switch (value)
                    {
                        case "0":
                        case "Success":
                            return 0;
                        case "1":
                        case "Error":
                            return 1;
                        default:
                            throw new JsonException($"Unknown string discriminator: {value}");
                    }
                default:
                    throw new JsonException(
                        $"Unsupported discriminator type: {(discriminatorValue != null ? discriminatorValue.GetType().Name : "null")}");
            }
        }

        private static object GetDiscriminatorValue(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Number:
                    return element.TryGetInt32(out var intValue) ? intValue : element.GetInt64();
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                default:
                    throw new JsonException($"Unexpected discriminator type: {element.ValueKind}");
            }
        }

        private static JsonSerializerOptions CreateOptionsWithoutThisConverter(JsonSerializerOptions originalOptions)
        {
            var newOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = originalOptions.PropertyNameCaseInsensitive,
                PropertyNamingPolicy = originalOptions.PropertyNamingPolicy,
                WriteIndented = originalOptions.WriteIndented,
                DictionaryKeyPolicy = originalOptions.DictionaryKeyPolicy,
            };

            foreach (var converter in originalOptions.Converters)
            {
                if (converter.GetType() != typeof(QueryResponseConverter))
                {
                    newOptions.Converters.Add(converter);
                }
            }

            return newOptions;
        }
    }
}
