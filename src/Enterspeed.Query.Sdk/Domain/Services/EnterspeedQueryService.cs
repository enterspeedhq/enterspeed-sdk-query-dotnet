using Enterspeed.Query.Sdk.Api.Models;
using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Domain.Connection;
using Enterspeed.Query.Sdk.Domain.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Enterspeed.Query.Sdk.Domain.Services
{
    public class EnterspeedQueryService : BaseEnterspeedQueryService, IEnterspeedQueryService
    {
        private readonly IJsonSerializer _serializer;

        public EnterspeedQueryService(
            EnterspeedQueryConnection enterspeedQueryConnection,
            IEnterspeedQueryConfigurationProvider queryConfigurationProvider,
            IJsonSerializer jsonSerializer)
            : base(enterspeedQueryConnection, queryConfigurationProvider)
        {
            _serializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
        }

        public async Task<QueryApiResponse> Query(string apiKey, string index, QueryObject query, CancellationToken? cancellationToken = null)
        {
            Validate(apiKey);

            var requestUri = RequestUri(index);

            var httpContent = new StringContent(_serializer.Serialize(query), Encoding.UTF8, "application/json");
            return await QueryApiResponseSingle(apiKey, requestUri, httpContent, cancellationToken);
        }

        public async Task<QueryApiResponse<IContent>> QueryTyped(string apiKey, string index, QueryObject query, CancellationToken? cancellationToken = null)
        {
            Validate(apiKey);

            var requestUri = RequestUri(index);

            var httpContent = new StringContent(_serializer.Serialize(query), Encoding.UTF8, "application/json");
            return await QueryApiResponseTypedSingle(apiKey, requestUri, httpContent, cancellationToken);
        }

        public async Task<MultiQueryApiResponse> Query(string apiKey, List<MultiQueryObject> queries, CancellationToken? cancellationToken = null)
        {
            Validate(apiKey);

            var requestUri = RequestUri();

            var httpContent = new StringContent(_serializer.Serialize(queries), Encoding.UTF8, "application/json");
            return await QueryApiResponseMultiple(apiKey, requestUri, httpContent, cancellationToken);
        }

        public async Task<MultiQueryApiResponse<IContent>> QueryTyped(string apiKey, List<MultiQueryObject> queries, CancellationToken? cancellationToken = null)
        {
            Validate(apiKey);

            var requestUri = RequestUri();

            var httpContent = new StringContent(_serializer.Serialize(queries), Encoding.UTF8, "application/json");
            return await QueryApiResponseTypedMultiple(apiKey, requestUri, httpContent, cancellationToken);
        }

        private async Task<QueryApiResponse<IContent>> QueryApiResponseTypedSingle(string apiKey, Uri requestUri, HttpContent content, CancellationToken? cancellationToken = null)
        {
            content.Headers.Add("X-Api-Key", apiKey);

            var response = await PostAsync(requestUri, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync();

            return new QueryApiResponse<IContent>
            {
                StatusCode = response.StatusCode,
                Message = response.StatusCode != HttpStatusCode.OK && !string.IsNullOrWhiteSpace(responseString)
                    ? _serializer.Deserialize<QueryApiError>(responseString)?.Message
                    : null,
                Response = response.StatusCode == HttpStatusCode.OK
                    ? _serializer.Deserialize<QueryResponse<IContent>>(responseString)
                    : null,
                Headers = response.Headers
            };
        }

        private async Task<MultiQueryApiResponse<IContent>> QueryApiResponseTypedMultiple(string apiKey, Uri requestUri, HttpContent content, CancellationToken? cancellationToken = null)
        {
            content.Headers.Add("X-Api-Key", apiKey);

            var response = await PostAsync(requestUri, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync();

            return new MultiQueryApiResponse<IContent>
            {
                StatusCode = response.StatusCode,
                Message = response.StatusCode != HttpStatusCode.OK && !string.IsNullOrWhiteSpace(responseString)
                    ? _serializer.Deserialize<QueryApiError>(responseString)?.Message
                    : null,
                Response = response.StatusCode == HttpStatusCode.OK
                    ? _serializer.Deserialize<List<MultiQueryResponse<IContent>>>(responseString)
                    : null,
                Headers = response.Headers
            };
        }

        private async Task<QueryApiResponse> QueryApiResponseSingle(string apiKey, Uri requestUri, HttpContent content, CancellationToken? cancellationToken = null)
        {
            content.Headers.Add("X-Api-Key", apiKey);
            
            var response = await PostAsync(requestUri, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync();

            return new QueryApiResponse
            {
                StatusCode = response.StatusCode,
                Message = response.StatusCode != HttpStatusCode.OK && !string.IsNullOrWhiteSpace(responseString)
                    ? _serializer.Deserialize<QueryApiError>(responseString)?.Message
                    : null,
                Response = response.StatusCode == HttpStatusCode.OK
                    ? _serializer.Deserialize<QueryResponse>(responseString)
                    : null,
                Headers = response.Headers
            };
        }

        private async Task<MultiQueryApiResponse> QueryApiResponseMultiple(string apiKey, Uri requestUri, HttpContent content, CancellationToken? cancellationToken = null)
        {
            content.Headers.Add("X-Api-Key", apiKey);

            var response = await PostAsync(requestUri, content, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync();

            return new MultiQueryApiResponse
            {
                StatusCode = response.StatusCode,
                Message = response.StatusCode != HttpStatusCode.OK && !string.IsNullOrWhiteSpace(responseString)
                    ? _serializer.Deserialize<QueryApiError>(responseString)?.Message
                    : null,
                Response = response.StatusCode == HttpStatusCode.OK
                    ? _serializer.Deserialize<List<MultiQueryResponse>>(responseString)
                    : null,
                Headers = response.Headers
            };
        }
    }
}