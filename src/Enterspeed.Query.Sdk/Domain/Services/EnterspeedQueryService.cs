using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Domain.Builders.MultiQuery;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse.MultiQuery;

namespace Enterspeed.Query.Sdk.Domain.Services
{
    public class EnterspeedQueryService : BaseEnterspeedQueryService, IEnterspeedQueryService
    {
        private readonly IJsonSerializer _serializer;

        public EnterspeedQueryService(
            HttpClient httpClient,
            IEnterspeedQueryConfigurationProvider queryConfigurationProvider,
            IJsonSerializer jsonSerializer)
            : base(httpClient, queryConfigurationProvider)
        {
            _serializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        }

        public async Task<MultiQueryApiResponse> Query(
            string apiKey,
            QueryRequest queries,
            CancellationToken? cancellationToken = null)
        {
            Validate(apiKey);

            var requestUri = RequestUri();

            // TODO: Should we have a Build in here if they forget to call it?
            var httpContent = new StringContent(_serializer.Serialize(queries.Queries), Encoding.UTF8, "application/json");

            Console.WriteLine("Serialized multi-query request content:"); // TODO REMOVE!
            Console.WriteLine(await httpContent.ReadAsStringAsync());
            return await QueryApiResponseMultiple(apiKey, requestUri, httpContent, cancellationToken);
        }

        private async Task<MultiQueryApiResponse> QueryApiResponseMultiple(string apiKey, Uri requestUri,
            HttpContent content, CancellationToken? cancellationToken = null)
        {
            try
            {
                content.Headers.Add("X-Api-Key", apiKey);
                var httpResponse = await PostAsync(requestUri, content, cancellationToken);
                var responseString = await httpResponse.Content.ReadAsStringAsync();

                // Deserialize the response - works for both OK (mixed responses) and BadRequest (all errors)
                var response = _serializer.Deserialize<List<MultiQueryResponse>>(responseString);

                // Convert to SDK response type using extension method
                var convertedResponses = response.ToMultiQueryResponse();

                if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Extract error messages from all error responses
                    var errorMessages = response
                        .OfType<MultiQueryResponseError>()
                        .Select(r => r.Message)
                        .Where(m => !string.IsNullOrEmpty(m))
                        .ToList();

                    var message = errorMessages.Any()
                        ? errorMessages.First() // Use first error message for the main message
                        : "All queries failed";

                    return new MultiQueryApiResponse
                    {
                        StatusCode = httpResponse.StatusCode,
                        Headers = httpResponse.Headers,
                        Message = message,
                        Response = convertedResponses
                    };
                }

                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new MultiQueryApiResponse
                    {
                        StatusCode = httpResponse.StatusCode,
                        Headers = httpResponse.Headers,
                        Message = "", // Empty message for successful responses
                        Response = convertedResponses
                    };
                }

                // Handle other HTTP status codes
                return new MultiQueryApiResponse
                {
                    StatusCode = httpResponse.StatusCode,
                    Headers = httpResponse.Headers,
                    Message = $"Unexpected status code: {httpResponse.StatusCode}",
                    Response = convertedResponses
                };

            }
            catch (Exception ex)
            {
                return new MultiQueryApiResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Message = $"Failed to deserialize multi query response: {ex.Message}",
                    Response = null
                };
            }
        }
    }
}
