using System;
using Enterspeed.Query.Sdk.Configuration;

namespace Enterspeed.Query.Sdk.Api.Providers
{
    public class EnterspeedQueryConfigurationProvider : IEnterspeedQueryConfigurationProvider
    {
        public EnterspeedQueryConfiguration Configuration { get; set; }

        public EnterspeedQueryConfigurationProvider(EnterspeedQueryConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
    }
}