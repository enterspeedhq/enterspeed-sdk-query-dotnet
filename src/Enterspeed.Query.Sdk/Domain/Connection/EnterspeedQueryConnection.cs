using System;
using System.Net.Http;
using Enterspeed.Query.Sdk.Api.Connection;
using Enterspeed.Query.Sdk.Api.Extensions;
using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Configuration;

namespace Enterspeed.Query.Sdk.Domain.Connection
{
    public sealed class EnterspeedQueryConnection : IEnterspeedDeliveryConnection
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl;

        public EnterspeedQueryConnection(
            IHttpClientFactory httpClientFactory,
            IEnterspeedQueryConfigurationProvider queryConfigurationProvider)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

            if (queryConfigurationProvider == null)
            {
                throw new ArgumentNullException(nameof(queryConfigurationProvider));
            }

            _baseUrl = queryConfigurationProvider.Configuration?.BaseUrl;
        }

        public HttpClient HttpClientConnection => string.IsNullOrWhiteSpace(_baseUrl)
            ? throw new ConfigurationException(nameof(_baseUrl))
            : _httpClientFactory.CreateClient(EnterspeedServiceCollectionExtension.HttpClientName);

        public void Flush()
        {
            // No-op: HttpClient lifecycle is managed by IHttpClientFactory
        }
    }
}
