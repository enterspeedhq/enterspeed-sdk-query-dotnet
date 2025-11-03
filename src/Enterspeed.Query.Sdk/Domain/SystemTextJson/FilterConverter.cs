using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Domain.Models;

namespace Enterspeed.Query.Sdk.Domain.SystemTextJson
{
    internal class FilterConverter : JsonConverter<IOperator>
    {
        public override IOperator Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Enterspeed Query SDK should not read any JSON.");
        }

        public override void Write(Utf8JsonWriter writer, IOperator value, JsonSerializerOptions options)
        {
            // Cast the IOperator to object and serialize to ensure the correct derived type is used
            var objectValue = value as object;
            JsonSerializer.Serialize(writer, objectValue, objectValue.GetType(), options);
        }
    }
}