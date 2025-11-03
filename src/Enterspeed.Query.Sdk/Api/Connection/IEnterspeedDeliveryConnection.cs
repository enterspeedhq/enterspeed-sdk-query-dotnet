using System.Net.Http;

namespace Enterspeed.Query.Sdk.Api.Connection
{
    public interface IEnterspeedDeliveryConnection
    {
        /// <summary>
        /// Gets the configured HttpClient.
        /// </summary>
        HttpClient HttpClientConnection { get; }

        /// <summary>
        /// Flushes/resets the current HttpClient.
        /// </summary>
        void Flush();
    }
}