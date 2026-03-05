using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Configuration;

namespace Enterspeed.Query.Sdk.Domain.Services
{
    public abstract class BaseEnterspeedQueryService
    {
        private readonly HttpClient _httpClient;
        private readonly IEnterspeedQueryConfigurationProvider _queryConfigurationProvider;

        protected BaseEnterspeedQueryService(
            HttpClient httpClient,
            IEnterspeedQueryConfigurationProvider queryConfigurationProvider)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _queryConfigurationProvider = queryConfigurationProvider ?? throw new ArgumentNullException(nameof(queryConfigurationProvider));
        }

        protected Uri RequestUri(string indexName = null)
        {
            var baseUrl = _queryConfigurationProvider.Configuration?.BaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ConfigurationException(nameof(EnterspeedQueryConfiguration.BaseUrl));
            }

            var requestUri = new Uri(baseUrl + $"/v{_queryConfigurationProvider.Configuration.QueryVersion}");

            if (string.IsNullOrWhiteSpace(indexName))
            {
                return requestUri;
            }

            var uriBuilder = new UriBuilder(requestUri);
            uriBuilder.Path += $"/{indexName}";
            requestUri = uriBuilder.Uri;

            return requestUri;
        }

        protected void Validate(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey), "API key must be set");
            }

            if (string.IsNullOrWhiteSpace(_queryConfigurationProvider.Configuration.QueryVersion))
            {
                throw new ConfigurationException(nameof(EnterspeedQueryConfiguration.QueryVersion));
            }
        }

        protected async Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content, CancellationToken? cancellationToken = null)
        {
            if (cancellationToken.HasValue)
            {
                return await _httpClient.PostAsync(requestUri, content, cancellationToken.Value);
            }

            return await _httpClient.PostAsync(requestUri, content);
        }
    }
}
