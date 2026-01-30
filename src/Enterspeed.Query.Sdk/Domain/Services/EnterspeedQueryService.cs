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
using Enterspeed.Query.Sdk.Domain.QueryApiResponse.Query;

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
            return QueryTyped<Dictionary<string, object>>(apiKey, index, query, cancellationToken);
        }

        public async Task<QueryApiResponse<T>> QueryTyped<T>(string apiKey, string index, QueryObject query,
            CancellationToken? cancellationToken = null)
        {
            Validate(apiKey);

            var requestUri = RequestUri(index);

            var httpContent = new StringContent(_serializer.Serialize(query), Encoding.UTF8, "application/json");
            return await QueryApiResponseSingle<T>(apiKey, requestUri, index, httpContent, cancellationToken);
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
        /// POST and deserialize single query responses.
        /// Uses polymorphic deserialization and converts to IResponse directly.
        /// </summary>
        private async Task<QueryApiResponse<T>> QueryApiResponseSingle<T>(
            string apiKey,
            Uri requestUri,
            string index,
            HttpContent content,
            CancellationToken? cancellationToken = null)
        {
            content.Headers.Add("X-Api-Key", apiKey);

            var httpResponse = await PostAsync(requestUri, content, cancellationToken);
            var responseString = await httpResponse.Content.ReadAsStringAsync();

            try
            {
                var response = _serializer.Deserialize<QueryResponse<T>>(responseString);
                // Convert to SDK response type

                if (response is QueryResponseSuccess<T> successResponse)
                {
                    var apiResponse = new QueryApiResponse<T>
                    {
                        StatusCode = httpResponse.StatusCode,
                        Headers = httpResponse.Headers,
                        Response = new SuccessResponse<T>(successResponse)
                    };
                    return apiResponse;
                }

                if (response is QueryResponseError<T> errorResponse)
                {
                    var apiResponse = new QueryApiResponse<T>
                    {
                        StatusCode = httpResponse.StatusCode,
                        Headers = httpResponse.Headers,
                        Message = errorResponse.Message,
                        Response =  new ErrorResponse<T>(errorResponse.ToQueryError(index))
                    };
                    return apiResponse;
                }
            } catch (Exception ex)
            {
                return new QueryApiResponse<T>
                {
                    StatusCode = httpResponse.StatusCode,
                    Headers = httpResponse.Headers,
                    Message = $"Failed to deserialize response: {ex.Message}",
                    Response = new ErrorResponse<T>(new QueryError { Message = $"Failed to deserialize response: {ex.Message}" })
                };
            }

            return new QueryApiResponse<T>
            {
                StatusCode = httpResponse.StatusCode,
                Headers = httpResponse.Headers,
                Message = "Unknown response type",
                Response = new ErrorResponse<T>(new QueryError { Message = "Unknown response type" })
            };
        }

        private async Task<MultiQueryApiResponse> QueryApiResponseMultiple(string apiKey, Uri requestUri,
            HttpContent content, CancellationToken? cancellationToken = null)
        {
            try
            {
                content.Headers.Add("X-Api-Key", apiKey);
                var httpResponse = await PostAsync(requestUri, content, cancellationToken);
                var responseString = await httpResponse.Content.ReadAsStringAsync();

                var response = _serializer.Deserialize<List<MultiQueryResponse>>(responseString);

                // Convert to SDK response type using extension method
                var convertedResponses = response.ToMultiQueryResponse();

                // Convert to SDK response type
                if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return new MultiQueryApiResponse
                    {
                        StatusCode = httpResponse.StatusCode,
                        Headers = httpResponse.Headers,
                        Message = httpResponse.StatusCode == System.Net.HttpStatusCode.OK ? "" : "No valid responses received, Bad Request",
                        Response = convertedResponses
                    };
                }

            } catch (Exception ex)
            {
                return new MultiQueryApiResponse
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Message = $"Failed to deserialize multi query response: {ex.Message}",
                    Response = null
                };
            }

            return new MultiQueryApiResponse
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Message = $"Failed with unknown error",
                Response = null
            };
        }
    }
}
