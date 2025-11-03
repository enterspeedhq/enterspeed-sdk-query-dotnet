using System;
using Enterspeed.Query.Sdk.Api.Providers;
using Enterspeed.Query.Sdk.Configuration;

namespace Enterspeed.Query.Sdk.Domain.Providers
{
    public class InMemoryQueryConfigurationProvider : IEnterspeedQueryConfigurationProvider
    {
        public InMemoryQueryConfigurationProvider(EnterspeedQueryConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public EnterspeedQueryConfiguration Configuration { get; }
    }
}