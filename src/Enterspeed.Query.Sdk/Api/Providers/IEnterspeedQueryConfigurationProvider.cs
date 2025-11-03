using Enterspeed.Query.Sdk.Configuration;

namespace Enterspeed.Query.Sdk.Api.Providers
{
    public interface IEnterspeedQueryConfigurationProvider
    {
        EnterspeedQueryConfiguration Configuration { get; }
    }
}