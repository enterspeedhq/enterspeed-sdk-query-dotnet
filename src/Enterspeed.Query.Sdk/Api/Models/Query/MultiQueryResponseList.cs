using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Enterspeed.Query.Sdk.Api.Models.MultiQuery;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;

namespace Enterspeed.Query.Sdk.Api.Models.Query
{
    // TODO: This class needs a custom converter to properly deserialize the different response types
    //[JsonConverter(typeof(QueryResponseConverter))]

    public class MultiQueryResponseList
    {
        //private readonly Dictionary<string, MultiQueryResponse> _queries = new();
        private readonly List<MultiQueryResponse> _results;

        public MultiQueryResponseList()
        {
            _results = new List<MultiQueryResponse>();
        }

        public MultiQueryResponseList(List<MultiQueryResponse> results)
        {
            _results = results ?? new List<MultiQueryResponse>();
        }

        public IReadOnlyList<MultiQueryResponseSuccess> GetSuccessResponse() =>
            _results.OfType<MultiQueryResponseSuccess>().ToList().AsReadOnly();

        public IReadOnlyList<MultiQueryResponseError> GetErrorResponse() =>
            _results.OfType<MultiQueryResponseError>().ToList().AsReadOnly();

        // Get Typed Object from our response
        public IReadOnlyList<T> GetResults<T>(string index)
        {
            // Get the Object by query name if it is in the success responses
            var matchedResults =
                GetSuccessResponse().FirstOrDefault(x => x.Index == index);
            if (matchedResults == null)
                throw new KeyNotFoundException($"Query '{index}' not found");

            var serializer = new SystemTextJsonSerializer();
            var typedResults = matchedResults.Results
                .Select(dict => serializer.Deserialize<T>(serializer.Serialize(dict)))
                .Where(item => item != null)
                .ToList();

            return typedResults.AsReadOnly();
        }


        /// <summary>
        /// Try get results safely
        /// </summary>
        public bool TryGetResults<T>(string index, out IReadOnlyList<T> results) // ReadONlyList<T> results)
        {
            results = null;
            try
            {
                results = GetResults<T>(index);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryGetResults(string index, out IReadOnlyList<Dictionary<string, object>> results) // ReadONlyList<T> results)
        {
            results = null;
            try
            {
                results = GetResults<Dictionary<string, object>>(index);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
