using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Domain.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Models.Response;
using Enterspeed.Query.Sdk.Domain.QueryApiResponse;

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

        public Task<QueryApiResponse<Dictionary<string, object>>> Query(string apiKey, string index, QueryObject query,
            CancellationToken? cancellationToken = null)
        {
            Validate(apiKey);

            var requestUri = RequestUri(index);

            var httpContent = new StringContent(_serializer.Serialize(query), Encoding.UTF8, "application/json");
            return await PostAndDeserializeAsync<IContent>(apiKey, requestUri, httpContent, cancellationToken);
        }

        public async Task<QueryApiResponse<T>> QueryTyped<T>(string apiKey, string index, QueryObject query,
            CancellationToken? cancellationToken = null)
        {
            Validate(apiKey);

            var requestUri = RequestUri(index);

            var httpContent = new StringContent(_serializer.Serialize(query), Encoding.UTF8, "application/json");
            return await PostAndDeserializeAsync<T>(apiKey, requestUri, httpContent, cancellationToken);
        }

        public async Task<MultiQueryApiResponse> Query(string apiKey, List<MultiQueryObject> queries,
            CancellationToken? cancellationToken = null)
        {
            Validate(apiKey);

            var requestUri = RequestUri();

            var httpContent = new StringContent(_serializer.Serialize(queries), Encoding.UTF8, "application/json");
            return await QueryApiResponseMultiple(apiKey, requestUri, httpContent, cancellationToken);
        }

        /// <summary>
        /// Unified method to POST and deserialize single query responses.
        /// Handles both success and error responses cleanly.
        /// </summary>
        private async Task<QueryApiResponse<T>> PostAndDeserializeAsync<T>(
            string apiKey,
            Uri requestUri,
            HttpContent content,
            CancellationToken? cancellationToken = null)
        {
            content.Headers.Add("X-Api-Key", apiKey);

            var response = await PostAsync(requestUri, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync();

            var apiResponse = new QueryApiResponse<T>
            {
                StatusCode = response.StatusCode,
                Headers = response.Headers
            };

            if (response.StatusCode == HttpStatusCode.OK)
            {
                // Try to deserialize as success response
                try
                {
                    apiResponse.RawResponse = _serializer.Deserialize<QueryResponseSuccess<T>>(responseString);
                }
                catch
                {
                    // If typed deserialization fails, it might be an error response
                    var errorResponse = _serializer.Deserialize<QueryResponseError>(responseString);
                    if (errorResponse != null)
                    {
                        apiResponse.RawResponse = errorResponse as IQueryResponse<T>;
                        apiResponse.Message = errorResponse.Message;
                    }
                }
            }
            else
            {
                // HTTP error
                var errorResponse = !string.IsNullOrWhiteSpace(responseString)
                    ? _serializer.Deserialize<QueryApiError>(responseString)
                    : null;

                apiResponse.Message = errorResponse?.Message ?? $"HTTP {(int)response.StatusCode}";
            }

            return apiResponse;
        }

        private async Task<MultiQueryApiResponse> QueryApiResponseMultiple(string apiKey, Uri requestUri,
            HttpContent content, CancellationToken? cancellationToken = null)
        {
            try
            {
                content.Headers.Add("X-Api-Key", apiKey);
                var response = await PostAsync(requestUri, content, cancellationToken);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new MultiQueryApiResponse
                    {
                        StatusCode = response.StatusCode,
                        Message = "Error",
                        Response = null,
                        Headers = response.Headers
                    };
                }

                var multiq = _serializer.Deserialize<List<MultiQueryResponse>>(responseString);

                var response1 = new MultiQueryApiResponse
                {
                    StatusCode = response.StatusCode,
                    Message = string.Join("; ", multiq
                        .Where(x => x is MultiQueryResponseError)
                        .Cast<MultiQueryResponseError>()
                        .SelectMany(x => new[] { x.Message }.Concat(x.Errors ?? Array.Empty<string>()))
                        .Where(x => !string.IsNullOrEmpty(x))),
                    Response = new MultiQueryResponseList(multiq),
                    Headers = response.Headers
                };
                return response1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new MultiQueryApiResponse();
        }
    }


}
