using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Api.Services;
using Enterspeed.Query.Sdk.Configuration;
using Enterspeed.Query.Sdk.Domain.Connection;
using Enterspeed.Query.Sdk.Domain.Services;
using Enterspeed.Query.Sdk.Domain.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;

namespace Enterspeed.Query.Sdk.Api.Extensions
{
    public static class EnterspeedServiceCollectionExtension
    {
        public static IServiceCollection AddEnterspeedQueryService(this IServiceCollection services, EnterspeedQueryConfiguration enterspeedQueryConfiguration = null)
        {
            services.AddTransient<IEnterspeedQueryService, EnterspeedQueryService>();
            services.AddTransient<IJsonSerializer, SystemTextJsonSerializer>();
            services.AddTransient<EnterspeedQueryConnection>();
            services.AddSingleton<IEnterspeedQueryConfigurationProvider>(new EnterspeedQueryConfigurationProvider(enterspeedQueryConfiguration ?? new EnterspeedQueryConfiguration()));
            return services;
        }
    }
}
