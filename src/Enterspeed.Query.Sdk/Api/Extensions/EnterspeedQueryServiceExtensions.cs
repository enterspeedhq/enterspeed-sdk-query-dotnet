using System;
using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Configuration;
using Enterspeed.Query.Sdk.Domain.Services;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;

namespace Enterspeed.Query.Sdk.Api.Extensions
{
    public static class EnterspeedQueryServiceExtensions
    {
        public static IServiceCollection AddEnterspeedQueryService(this IServiceCollection services, EnterspeedQueryConfiguration enterspeedQueryConfiguration = null)
        {
            var configuration = enterspeedQueryConfiguration ?? new EnterspeedQueryConfiguration();
            var configurationProvider = new EnterspeedQueryConfigurationProvider(configuration);

            services.AddTransient<IJsonSerializer, SystemTextJsonSerializer>();
            services.AddSingleton<IEnterspeedQueryConfigurationProvider>(configurationProvider);

            services.AddHttpClient<IEnterspeedQueryService, EnterspeedQueryService>(client =>
            {
                if (!string.IsNullOrWhiteSpace(configuration.BaseUrl))
                {
                    client.BaseAddress = new Uri(configuration.BaseUrl);
                }
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(configuration.ConnectionTimeout > 0 ? configuration.ConnectionTimeout : 60);
            })
            .SetHandlerLifetime(TimeSpan.FromSeconds(configuration.ConnectionTimeout > 0 ? configuration.ConnectionTimeout : 60));

            return services;
        }
    }
}
