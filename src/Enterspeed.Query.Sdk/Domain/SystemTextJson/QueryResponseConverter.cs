using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query;

namespace Enterspeed.Query.Sdk.Domain.SystemTextJson
{
    /// <summary>
    /// Custom converter for polymorphic QueryResponse that handles discriminator
    /// property appearing in any position (not just first)
    /// </summary>
    public class QueryResponseConverter : JsonConverter<QueryResponse>
    {
        private const string DiscriminatorPropertyName = "status";

        public override QueryResponse Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var root = jsonDoc.RootElement;

                // Extract discriminator value from anywhere in the JSON
                if (!root.TryGetProperty(DiscriminatorPropertyName, out var discriminatorElement))
                {
                    throw new JsonException($"Missing required discriminator property '{DiscriminatorPropertyName}'");
                }

                // Get the numeric or string discriminator value
                var discriminatorValue = GetDiscriminatorValue(discriminatorElement);

                // Determine which concrete type to deserialize into
                Type targetType;
                switch (discriminatorValue)
                {
                    case 0:
                    case (long)0:
                        targetType = typeof(QueryResponseSuccess);
                        break;
                    case 1:
                    case (long)1:
                        targetType = typeof(QueryResponseError);
                        break;
                    default:
                        throw new JsonException($"Unknown discriminator value: {discriminatorValue}");
                }

                // Deserialize using the concrete type
                var concreteResponse = JsonSerializer.Deserialize(
                    root.GetRawText(),
                    targetType,
                    options);

                return (QueryResponse)concreteResponse;
            }
        }

        public override void Write(Utf8JsonWriter writer, QueryResponse value, JsonSerializerOptions options)
        {
            // Ensure discriminator is written first
            JsonSerializer.Serialize<object>(writer, value, options);
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
    }

    /// <summary>
    /// Generic custom converter for polymorphic QueryResponse&lt;T&gt; that handles discriminator
    /// property appearing in any position (not just first)
    /// </summary>
    public class QueryResponseConverter<T> : JsonConverter<QueryResponse<T>>
    {
        private const string DiscriminatorPropertyName = "status";

        public override QueryResponse<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var root = jsonDoc.RootElement;

                // Extract discriminator value from anywhere in the JSON
                if (!root.TryGetProperty(DiscriminatorPropertyName, out var discriminatorElement))
                {
                    throw new JsonException($"Missing required discriminator property '{DiscriminatorPropertyName}'");
                }

                // Get the numeric or string discriminator value
                var discriminatorValue = GetDiscriminatorValue(discriminatorElement);

                // Determine which concrete type to deserialize into
                Type targetType;
                switch (discriminatorValue)
                {
                    case 0:
                    case (long)0:
                        targetType = typeof(QueryResponseSuccess<T>);
                        break;
                    case 1:
                    case (long)1:
                        targetType = typeof(QueryResponseError<T>);
                        break;
                    default:
                        throw new JsonException($"Unknown discriminator value: {discriminatorValue}");
                }

                // Create options without this converter to avoid infinite recursion
                var optionsWithoutConverter = CreateOptionsWithoutThisConverter(options);

                // Deserialize using the concrete type
                var concreteResponse = JsonSerializer.Deserialize(
                    root.GetRawText(),
                    targetType,
                    optionsWithoutConverter);

                return (QueryResponse<T>)concreteResponse;
            }
        }

        public override void Write(Utf8JsonWriter writer, QueryResponse<T> value, JsonSerializerOptions options)
        {
            // Ensure discriminator is written first
            JsonSerializer.Serialize<object>(writer, value, options);
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

        /// <summary>
        /// Create a new JsonSerializerOptions that excludes this converter to avoid infinite recursion
        /// </summary>
        private static JsonSerializerOptions CreateOptionsWithoutThisConverter(JsonSerializerOptions originalOptions)
        {
            var newOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = originalOptions.PropertyNameCaseInsensitive,
                PropertyNamingPolicy = originalOptions.PropertyNamingPolicy,
                WriteIndented = originalOptions.WriteIndented,
                DictionaryKeyPolicy = originalOptions.DictionaryKeyPolicy,
            };

            // Copy converters EXCEPT this converter type
            foreach (var converter in originalOptions.Converters)
            {
                // Skip QueryResponseConverterFactory and QueryResponseConverter<T> to prevent recursion
                var converterType = converter.GetType();
                if (converterType != typeof(QueryResponseConverterFactory) &&
                    (!converterType.IsGenericType || converterType.GetGenericTypeDefinition() != typeof(QueryResponseConverter<>)))
                {
                    newOptions.Converters.Add(converter);
                }
            }

            return newOptions;
        }
    }

    /// <summary>
    /// Factory for creating generic QueryResponseConverter&lt;T&gt; instances
    /// </summary>
    public class QueryResponseConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            var genericType = typeToConvert.GetGenericTypeDefinition();
            return genericType == typeof(QueryResponse<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var elementType = typeToConvert.GetGenericArguments()[0];
            var converterType = typeof(QueryResponseConverter<>).MakeGenericType(elementType);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }

    /// <summary>
    /// Custom converter for polymorphic MultiQueryResponse (same pattern)
    /// </summary>
    public class MultiQueryResponseConverter : JsonConverter<MultiQueryResponse>
    {
        private const string DiscriminatorPropertyName = "status";

        public override MultiQueryResponse Read(ref Utf8JsonReader reader, Type typeToConvert,
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

                // Create a NEW JsonSerializerOptions WITHOUT this converter to avoid recursion
                var optionsWithoutConverter = CreateOptionsWithoutThisConverter(options);

                var concreteResponse = JsonSerializer.Deserialize(
                    root.GetRawText(),
                    targetType,
                    optionsWithoutConverter);

                return (MultiQueryResponse)concreteResponse;
            }
            finally
            {
                jsonDoc.Dispose();
            }
        }

        public override void Write(Utf8JsonWriter writer, MultiQueryResponse value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, options);
        }

        private Type GetTargetType(object discriminatorValue)
        {
            var normalizedValue = NormalizeDiscriminator(discriminatorValue);

            switch (normalizedValue)
            {
                case 0:
                    return typeof(MultiQueryResponseSuccess);
                case 1:
                    return typeof(MultiQueryResponseError);
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
                {
                    return (int)value;
                }
                case string value:
                {
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

        /// <summary>
        /// Create a new JsonSerializerOptions that excludes this converter to avoid infinite recursion
        /// </summary>
        private static JsonSerializerOptions CreateOptionsWithoutThisConverter(JsonSerializerOptions originalOptions)
        {
            var newOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = originalOptions.PropertyNameCaseInsensitive,
                PropertyNamingPolicy = originalOptions.PropertyNamingPolicy,
                WriteIndented = originalOptions.WriteIndented,
                DictionaryKeyPolicy = originalOptions.DictionaryKeyPolicy,
            };

            // Copy converters EXCEPT this one
            foreach (var converter in originalOptions.Converters)
            {
                // Skip this converter type to prevent recursion
                if (converter.GetType() != typeof(MultiQueryResponseConverter))
                {
                    newOptions.Converters.Add(converter);
                }
            }

            return newOptions;
        }
    }
}

