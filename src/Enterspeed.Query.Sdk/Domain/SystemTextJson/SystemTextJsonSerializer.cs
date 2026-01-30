﻿#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Api.Services;

namespace Enterspeed.Query.Sdk.Domain.SystemTextJson
{
    public class SystemTextJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public SystemTextJsonSerializer(IList<JsonConverter> converters = null)
        {
            if (converters != null && converters.Any())
            {
                foreach (var converter in converters)
                {
                    _options.Converters.Add(converter);
                }
            }
            else
            {
                // Default converters when none provided
                _options.Converters.Add(new FilterConverter());
                _options.Converters.Add(new SortOrderJsonConverter());
                _options.Converters.Add(new QueryResponseConverterFactory()); // For polymorphic QueryResponse<T>
                _options.Converters.Add(new MultiQueryResponseConverter()); // For polymorphic MultiQueryResponse
            }
        }

        public string Serialize(object value)
        {
            return JsonSerializer.Serialize(value, _options);
        }

        public T Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, _options);
        }
    }
}
#endif
