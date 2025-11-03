using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Configuration;
using Enterspeed.Query.Sdk.Domain.Connection;

namespace Enterspeed.Query.Sdk.Domain.Services
{
    public abstract class BaseEnterspeedQueryService
    {
        protected readonly EnterspeedQueryConnection EnterspeedQueryConnection;
        private readonly IEnterspeedQueryConfigurationProvider _queryConfigurationProvider;

        protected BaseEnterspeedQueryService(
            EnterspeedQueryConnection enterspeedQueryConnection,
            IEnterspeedQueryConfigurationProvider queryConfigurationProvider)
        {
            EnterspeedQueryConnection = enterspeedQueryConnection ?? throw new ArgumentNullException(nameof(enterspeedQueryConnection));
            _queryConfigurationProvider = queryConfigurationProvider ?? throw new ArgumentNullException(nameof(queryConfigurationProvider));
        }

        protected Uri RequestUri(string indexName = null)
        {
            var requestUri = new Uri(EnterspeedQueryConnection.HttpClientConnection.BaseAddress,
                $"/v{_queryConfigurationProvider.Configuration.QueryVersion}");

            if (!string.IsNullOrWhiteSpace(indexName))
            {
                var uriBuilder = new UriBuilder(requestUri);
                uriBuilder.Path += $"/{indexName}";
                requestUri = uriBuilder.Uri;
            }

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
            HttpResponseMessage response;
            if (cancellationToken.HasValue)
            {
                response = await EnterspeedQueryConnection.HttpClientConnection.PostAsync(requestUri, content, cancellationToken.Value);
            }
            else
            {
                response = await EnterspeedQueryConnection.HttpClientConnection.PostAsync(requestUri, content);
            }

            return response;
        }
    }
}